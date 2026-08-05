// --------------------------------------------------------------------------------------------------
// <copyright file="SaleCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Modules.Sales.Core.Exceptions;
using FluentPOS.Shared.Core.Enums;
using FluentPOS.Shared.Core.IntegrationServices.Application;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.IntegrationServices.Inventory;
using FluentPOS.Shared.Core.IntegrationServices.People;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands
{
    internal sealed class SaleCommandHandler :
        IRequestHandler<RegisterSaleCommand, Result<Guid>>,
        IRequestHandler<RefundSaleCommand, Result<Guid>>
    {
        private readonly IEntityReferenceService _referenceService;
        private readonly IStockService _stockService;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ISalesDbContext _salesContext;
        private readonly IStringLocalizer<SaleCommandHandler> _localizer;

        public SaleCommandHandler(
            IStringLocalizer<SaleCommandHandler> localizer,
            ISalesDbContext salesContext,
            ICartService cartService,
            IProductService productService,
            IStockService stockService,
            IEntityReferenceService referenceService)
        {
            _localizer = localizer;
            _salesContext = salesContext;
            _cartService = cartService;
            _productService = productService;
            _stockService = stockService;
            _referenceService = referenceService;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterSaleCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = Order.InitializeOrder();
            string referenceNumber = await _referenceService.TrackAsync(order.GetType().Name);
            order.SetReferenceNumber(referenceNumber);
            var cartDetails = await _cartService.GetDetailsAsync(command.CartId);

            // Do all mandatory null checks
            if (cartDetails?.Data == null) throw new Exception();
            if (cartDetails.Data.Customer == null) throw new Exception("Customer Invalid!");
            if (cartDetails.Data.CartItems == null) throw new Exception("Empty Cart!");
            var customer = cartDetails.Data.Customer;

            // The sale happens in the store the cart was opened in.
            Guid storeId = cartDetails.Data.StoreId;
            order.StoreId = storeId;

            if (command.TillSessionId.HasValue)
            {
                var session = await _salesContext.TillSessions
                    .FirstOrDefaultAsync(ts => ts.Id == command.TillSessionId.Value, cancellationToken);
                if (session == null || session.Status != TillSessionStatus.Open)
                {
                    throw new SalesException(_localizer["Till session not found or not open."], HttpStatusCode.BadRequest);
                }

                if (session.StoreId != storeId)
                {
                    throw new SalesException(_localizer["Till session belongs to a different store than the cart."], HttpStatusCode.BadRequest);
                }

                order.TillSessionId = session.Id;
            }

            order.AddCustomer(customer);
            bool requiresAgeVerification = false;
            foreach (var item in cartDetails.Data.CartItems)
            {
                var productResponse = await _productService.GetDetailsAsync(item.ProductId, storeId);
                if (productResponse.Succeeded)
                {
                    var product = productResponse.Data;
                    requiresAgeVerification |= product.IsAgeRestricted;
                    order.AddProduct(item.ProductId, product.Name, item.Quantity, product.Price, product.Tax);
                }
            }

            // Challenge 25: restricted items cannot be sold without an explicit age check.
            if (requiresAgeVerification)
            {
                if (!command.AgeVerified)
                {
                    throw new SalesException(_localizer["Age verification is required: the basket contains age-restricted items (Challenge 25)."], HttpStatusCode.BadRequest);
                }

                order.RecordAgeVerification();
            }

            order.MarkAsPaid();
            var transaction = Transaction.Record(order.Id, command.PaymentType, order.Total, command.TenderedAmount, command.Note, storeId, order.TillSessionId);

            await _salesContext.Orders.AddAsync(order, cancellationToken);
            await _salesContext.Transactions.AddAsync(transaction, cancellationToken);
            await _salesContext.SaveChangesAsync(cancellationToken);
            await _cartService.RemoveCartAsync(command.CartId);
            foreach (var product in order.Products)
            {
                await _stockService.RecordTransaction(product.ProductId, product.Quantity, order.ReferenceNumber, storeId);
            }

            return await Result<Guid>.SuccessAsync(order.Id, string.Format(_localizer["Order {0} Created"], order.ReferenceNumber));
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RefundSaleCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = await _salesContext.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
            if (order == null)
            {
                throw new SalesException(_localizer["Order Not Found!"], HttpStatusCode.NotFound);
            }

            if (command.TillSessionId.HasValue)
            {
                var session = await _salesContext.TillSessions
                    .FirstOrDefaultAsync(ts => ts.Id == command.TillSessionId.Value && ts.Status == TillSessionStatus.Open, cancellationToken);
                if (session == null || session.StoreId != order.StoreId)
                {
                    throw new SalesException(_localizer["Till session not found, not open, or belongs to a different store."], HttpStatusCode.BadRequest);
                }
            }

            try
            {
                order.MarkAsRefunded(command.Reason);
            }
            catch (InvalidOperationException ex)
            {
                throw new SalesException(ex.Message, HttpStatusCode.BadRequest);
            }

            // Reverse the payment. Original payment type is reused so till cash maths stay right.
            var originalPayment = await _salesContext.Transactions
                .Where(t => t.OrderId == order.Id)
                .OrderBy(t => t.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            var refund = Transaction.Record(
                order.Id,
                originalPayment?.PaymentType ?? PaymentType.Cash,
                -order.Total,
                0,
                $"Refund: {command.Reason}",
                order.StoreId,
                command.TillSessionId);

            await _salesContext.Transactions.AddAsync(refund, cancellationToken);
            await _salesContext.SaveChangesAsync(cancellationToken);

            // Goods go back on the shelf.
            foreach (var product in order.Products)
            {
                await _stockService.RecordTransaction(product.ProductId, product.Quantity, $"{order.ReferenceNumber}-R", order.StoreId, StockTransactionKind.Return);
            }

            return await Result<Guid>.SuccessAsync(order.Id, string.Format(_localizer["Order {0} Refunded"], order.ReferenceNumber));
        }
    }
}
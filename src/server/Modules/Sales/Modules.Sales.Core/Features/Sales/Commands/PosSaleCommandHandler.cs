// --------------------------------------------------------------------------------------------------
// <copyright file="PosSaleCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Modules.Sales.Core.Exceptions;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.IntegrationServices.Application;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.IntegrationServices.Inventory;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.IntegrationServices.People;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands
{
    internal sealed class PosSaleCommandHandler : IRequestHandler<RegisterPosSaleCommand, Result<Guid>>
    {
        private readonly ISalesDbContext _salesContext;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly ITenantContext _tenant;
        private readonly IEntityReferenceService _referenceService;
        private readonly IStringLocalizer<PosSaleCommandHandler> _localizer;

        public PosSaleCommandHandler(
            ISalesDbContext salesContext,
            IProductService productService,
            IStockService stockService,
            ICustomerService customerService,
            IStoreService storeService,
            ITenantContext tenant,
            IEntityReferenceService referenceService,
            IStringLocalizer<PosSaleCommandHandler> localizer)
        {
            _salesContext = salesContext;
            _productService = productService;
            _stockService = stockService;
            _customerService = customerService;
            _storeService = storeService;
            _tenant = tenant;
            _referenceService = referenceService;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterPosSaleCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            // Idempotent replay: the device-generated id is the order id, so a sale queued
            // offline and submitted twice results in exactly one order.
            if (await _salesContext.Orders.AnyAsync(o => o.Id == command.ClientSaleId, cancellationToken))
            {
                return await Result<Guid>.SuccessAsync(command.ClientSaleId, _localizer["Sale already recorded."]);
            }

            Guid storeId = command.StoreId ?? _tenant.StoreId ?? await _storeService.GetDefaultStoreIdAsync();
            if (_tenant.StoreId.HasValue && storeId != _tenant.StoreId.Value)
            {
                throw new SalesException(_localizer["You cannot record a sale for another store."], HttpStatusCode.Forbidden);
            }

            if (!await _storeService.ExistsAsync(storeId))
            {
                throw new SalesException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            var customer = await _customerService.GetDetailsAsync(command.CustomerId ?? OrganizationConstants.WalkInCustomerId);
            if (customer == null)
            {
                throw new SalesException(_localizer["Customer Not Found!"], HttpStatusCode.NotFound);
            }

            if (command.TillSessionId.HasValue)
            {
                var session = await _salesContext.TillSessions
                    .FirstOrDefaultAsync(ts => ts.Id == command.TillSessionId.Value, cancellationToken);
                if (session == null || session.Status != TillSessionStatus.Open || session.StoreId != storeId)
                {
                    throw new SalesException(_localizer["Till session not found, not open, or belongs to a different store."], HttpStatusCode.BadRequest);
                }
            }

            var order = Order.InitializeOrder();
            order.Id = command.ClientSaleId;
            order.StoreId = storeId;
            order.TillSessionId = command.TillSessionId;
            order.SetReferenceNumber(await _referenceService.TrackAsync(nameof(Order)));
            order.AddCustomer(customer);

            bool requiresAgeVerification = false;
            foreach (var item in command.Items)
            {
                var productResponse = await _productService.GetDetailsAsync(item.ProductId, storeId);
                if (!productResponse.Succeeded)
                {
                    throw new SalesException(_localizer["Product Not Found!"], HttpStatusCode.NotFound);
                }

                var product = productResponse.Data;
                requiresAgeVerification |= product.IsAgeRestricted;
                order.AddProduct(item.ProductId, product.Name, item.Quantity, product.Price, product.Tax);
            }

            if (requiresAgeVerification)
            {
                if (!command.AgeVerified)
                {
                    throw new SalesException(_localizer["Age verification is required: the basket contains age-restricted items (Challenge 25)."], HttpStatusCode.BadRequest);
                }

                order.RecordAgeVerification();
            }

            order.MarkAsPaid();
            var transaction = Transaction.Record(order.Id, command.PaymentType, order.Total, command.TenderedAmount, command.Note, storeId, command.TillSessionId);

            await _salesContext.Orders.AddAsync(order, cancellationToken);
            await _salesContext.Transactions.AddAsync(transaction, cancellationToken);
            await _salesContext.SaveChangesAsync(cancellationToken);

            foreach (var product in order.Products)
            {
                await _stockService.RecordTransaction(product.ProductId, product.Quantity, order.ReferenceNumber, storeId);
            }

            return await Result<Guid>.SuccessAsync(order.Id, string.Format(_localizer["Order {0} Created"], order.ReferenceNumber));
        }
    }
}

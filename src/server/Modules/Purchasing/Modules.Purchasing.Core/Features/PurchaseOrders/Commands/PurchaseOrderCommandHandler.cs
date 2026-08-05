// --------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrderCommandHandler.cs" company="FluentPOS">
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
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Modules.Purchasing.Core.Exceptions;
using FluentPOS.Shared.Core.Enums;
using FluentPOS.Shared.Core.IntegrationServices.Application;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.IntegrationServices.Inventory;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands
{
    internal class PurchaseOrderCommandHandler :
        IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>,
        IRequestHandler<SubmitPurchaseOrderCommand, Result<Guid>>,
        IRequestHandler<ReceivePurchaseOrderCommand, Result<Guid>>,
        IRequestHandler<CancelPurchaseOrderCommand, Result<Guid>>
    {
        private readonly IPurchasingDbContext _context;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly IStoreService _storeService;
        private readonly ITenantContext _tenant;
        private readonly IEntityReferenceService _referenceService;
        private readonly IStringLocalizer<PurchaseOrderCommandHandler> _localizer;

        public PurchaseOrderCommandHandler(
            IPurchasingDbContext context,
            IProductService productService,
            IStockService stockService,
            IStoreService storeService,
            ITenantContext tenant,
            IEntityReferenceService referenceService,
            IStringLocalizer<PurchaseOrderCommandHandler> localizer)
        {
            _context = context;
            _productService = productService;
            _stockService = stockService;
            _storeService = storeService;
            _tenant = tenant;
            _referenceService = referenceService;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            Guid storeId = command.StoreId ?? _tenant.StoreId ?? await _storeService.GetDefaultStoreIdAsync();
            if (_tenant.StoreId.HasValue && storeId != _tenant.StoreId.Value)
            {
                throw new PurchasingException(_localizer["You cannot create a purchase order for another store."], HttpStatusCode.Forbidden);
            }

            if (!await _storeService.ExistsAsync(storeId))
            {
                throw new PurchasingException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            if (command.SupplierId.HasValue && !await _context.Suppliers.AnyAsync(s => s.Id == command.SupplierId.Value, cancellationToken))
            {
                throw new PurchasingException(_localizer["Supplier Not Found!"], HttpStatusCode.NotFound);
            }

            var order = new PurchaseOrder
            {
                StoreId = storeId,
                SupplierId = command.SupplierId,
                Notes = command.Notes
            };
            order.SetReferenceNumber(await _referenceService.TrackAsync(nameof(PurchaseOrder)));

            foreach (var item in command.Items)
            {
                var productResponse = await _productService.GetDetailsAsync(item.ProductId);
                if (!productResponse.Succeeded)
                {
                    throw new PurchasingException(_localizer["Product Not Found!"], HttpStatusCode.NotFound);
                }

                order.AddItem(item.ProductId, productResponse.Data.Name, item.Quantity, item.UnitCost ?? productResponse.Data.Cost);
            }

            await _context.PurchaseOrders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(order.Id, string.Format(_localizer["Purchase Order {0} Created"], order.ReferenceNumber));
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = await GetOrderAsync(command.Id, cancellationToken);

            if (command.SupplierId.HasValue)
            {
                if (!await _context.Suppliers.AnyAsync(s => s.Id == command.SupplierId.Value, cancellationToken))
                {
                    throw new PurchasingException(_localizer["Supplier Not Found!"], HttpStatusCode.NotFound);
                }

                order.SupplierId = command.SupplierId;
            }

            try
            {
                order.Submit();
            }
            catch (InvalidOperationException ex)
            {
                throw new PurchasingException(ex.Message, HttpStatusCode.BadRequest);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(order.Id, _localizer["Purchase Order Submitted"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(ReceivePurchaseOrderCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = await GetOrderAsync(command.Id, cancellationToken);

            try
            {
                order.MarkAsReceived();
            }
            catch (InvalidOperationException ex)
            {
                throw new PurchasingException(ex.Message, HttpStatusCode.BadRequest);
            }

            foreach (var line in order.Items)
            {
                decimal receivedQuantity = command.Items?.FirstOrDefault(i => i.ProductId == line.ProductId)?.ReceivedQuantity ?? line.Quantity;
                if (receivedQuantity < 0)
                {
                    throw new PurchasingException(_localizer["Received quantity cannot be negative."], HttpStatusCode.BadRequest);
                }

                line.ReceivedQuantity = receivedQuantity;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Book the goods into stock after the order state is persisted.
            foreach (var line in order.Items.Where(l => l.ReceivedQuantity > 0))
            {
                await _stockService.RecordTransaction(line.ProductId, line.ReceivedQuantity, order.ReferenceNumber, order.StoreId, StockTransactionKind.Purchase);
            }

            return await Result<Guid>.SuccessAsync(order.Id, _localizer["Purchase Order Received"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(CancelPurchaseOrderCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = await GetOrderAsync(command.Id, cancellationToken);

            try
            {
                order.Cancel();
            }
            catch (InvalidOperationException ex)
            {
                throw new PurchasingException(ex.Message, HttpStatusCode.BadRequest);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(order.Id, _localizer["Purchase Order Cancelled"]);
        }

        private async Task<PurchaseOrder> GetOrderAsync(Guid id, CancellationToken cancellationToken)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
            if (order == null)
            {
                throw new PurchasingException(_localizer["Purchase Order Not Found!"], HttpStatusCode.NotFound);
            }

            return order;
        }
    }
}

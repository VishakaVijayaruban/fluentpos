// --------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrderQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Modules.Purchasing.Core.Enums;
using FluentPOS.Modules.Purchasing.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Purchasing.PurchaseOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Queries
{
    internal class PurchaseOrderQueryHandler :
        IRequestHandler<GetPurchaseOrdersQuery, Result<List<GetPurchaseOrdersResponse>>>,
        IRequestHandler<GetPurchaseOrderByIdQuery, Result<GetPurchaseOrdersResponse>>
    {
        private readonly IPurchasingDbContext _context;
        private readonly IStringLocalizer<PurchaseOrderQueryHandler> _localizer;

        public PurchaseOrderQueryHandler(
            IPurchasingDbContext context,
            IStringLocalizer<PurchaseOrderQueryHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetPurchaseOrdersResponse>>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var queryable = _context.PurchaseOrders.AsNoTracking().Include(po => po.Items).AsQueryable();
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(po => po.StoreId == request.StoreId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && System.Enum.TryParse<PurchaseOrderStatus>(request.Status, true, out var status))
            {
                queryable = queryable.Where(po => po.Status == status);
            }

            var orders = await queryable
                .OrderByDescending(po => po.TimeStamp)
                .ToListAsync(cancellationToken);

            return await Result<List<GetPurchaseOrdersResponse>>.SuccessAsync(orders.Select(MapOrder).ToList());
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<GetPurchaseOrdersResponse>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var order = await _context.PurchaseOrders.AsNoTracking()
                .Include(po => po.Items)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.Id == request.Id, cancellationToken);

            if (order == null)
            {
                throw new PurchasingException(_localizer["Purchase Order Not Found!"], HttpStatusCode.NotFound);
            }

            return await Result<GetPurchaseOrdersResponse>.SuccessAsync(MapOrder(order));
        }

        private static GetPurchaseOrdersResponse MapOrder(PurchaseOrder order)
        {
            return new GetPurchaseOrdersResponse(order.Id, order.StoreId, order.SupplierId, order.Supplier?.Name, order.ReferenceNumber, order.Status.ToString(), order.TimeStamp, order.Notes, order.Total)
            {
                Items = order.Items.Select(i => new PurchaseOrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitCost, i.ReceivedQuantity)).ToList()
            };
        }
    }
}

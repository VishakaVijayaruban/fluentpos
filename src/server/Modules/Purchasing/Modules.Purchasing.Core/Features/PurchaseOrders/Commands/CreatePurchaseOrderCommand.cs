// --------------------------------------------------------------------------------------------------
// <copyright file="CreatePurchaseOrderCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands
{
    public class CreatePurchaseOrderCommand : IRequest<Result<Guid>>
    {
        // Optional for store-scoped users (their token's store); required for head office.
        public Guid? StoreId { get; set; }

        public Guid? SupplierId { get; set; }

        public string Notes { get; set; }

        public List<CreatePurchaseOrderItem> Items { get; set; } = new();
    }

    public class CreatePurchaseOrderItem
    {
        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        // Optional; falls back to the product's central cost price.
        public decimal? UnitCost { get; set; }
    }
}

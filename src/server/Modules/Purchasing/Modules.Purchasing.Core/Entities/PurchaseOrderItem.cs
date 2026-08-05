// --------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrderItem.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Purchasing.Core.Entities
{
    public class PurchaseOrderItem : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }

        public Guid ProductId { get; set; }

        // Denormalized for readable orders even if the product is later renamed.
        public string ProductName { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal ReceivedQuantity { get; set; }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="Stock.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Inventory.Core.Entities
{
    public class Stock : BaseEntity, IMustHaveStore
    {
        public Stock()
        {
        }

        public Stock(Guid productId, Guid storeId)
        {
            ProductId = productId;
            StoreId = storeId;
            LastUpdatedOn = DateTime.UtcNow;
        }

        public Guid StoreId { get; set; }

        public Guid ProductId { get; private set; }

        public decimal AvailableQuantity { get; private set; }

        public DateTime LastUpdatedOn { get; private set; }

        public void IncreaseQuantity(decimal quantity)
        {
            AvailableQuantity += quantity;
            LastUpdatedOn = DateTime.UtcNow;
        }

        public void ReduceQuantity(decimal quantity)
        {
            AvailableQuantity -= quantity;
            LastUpdatedOn = DateTime.UtcNow;
        }
    }
}
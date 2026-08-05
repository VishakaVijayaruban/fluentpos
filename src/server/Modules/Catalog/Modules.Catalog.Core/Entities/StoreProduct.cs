// --------------------------------------------------------------------------------------------------
// <copyright file="StoreProduct.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Catalog.Core.Entities
{
    /// <summary>
    /// Per-store overlay on a centrally managed product: price override, ranging, and
    /// replenishment settings. Absence of a row means the store inherits central values.
    /// </summary>
    public class StoreProduct : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; }

        // Overrides the central sell price when set.
        public decimal? Price { get; set; }

        // False means the product is not sold in this store.
        public bool IsRanged { get; set; } = true;

        public decimal? ReorderPoint { get; set; }

        public decimal? ReorderQuantity { get; set; }
    }
}

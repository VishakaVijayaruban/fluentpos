// --------------------------------------------------------------------------------------------------
// <copyright file="ReorderCandidate.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Catalogs.StoreProducts
{
    /// <summary>
    /// A ranged store product with replenishment settings, used by the auto-replenishment job.
    /// </summary>
    public record ReorderCandidate(Guid StoreId, Guid ProductId, string ProductName, decimal UnitCost, decimal ReorderPoint, decimal ReorderQuantity, Guid? PreferredSupplierId);
}

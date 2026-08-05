// --------------------------------------------------------------------------------------------------
// <copyright file="GetStoreProductsResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Catalogs.StoreProducts
{
    public record GetStoreProductsResponse(Guid Id, Guid StoreId, Guid ProductId, string ProductName, decimal? Price, bool IsRanged, decimal? ReorderPoint, decimal? ReorderQuantity, Guid? PreferredSupplierId);
}

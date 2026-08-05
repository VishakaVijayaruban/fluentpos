// --------------------------------------------------------------------------------------------------
// <copyright file="CatalogSyncResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.DTOs.Catalogs.Products;
using FluentPOS.Shared.DTOs.Catalogs.StoreProducts;
using FluentPOS.Shared.DTOs.Catalogs.VatRates;

namespace FluentPOS.Shared.DTOs.Catalogs.Sync
{
    /// <summary>
    /// Incremental catalog changes since a cursor. Clients persist <see cref="ServerTime"/>
    /// as the next cursor — it is the server clock, so client clock skew never matters.
    /// </summary>
    public record CatalogSyncResponse(DateTime ServerTime)
    {
        public List<GetProductsResponse> Products { get; set; } = new();

        public List<GetStoreProductsResponse> StoreProducts { get; set; } = new();

        public List<GetVatRatesResponse> VatRates { get; set; } = new();
    }
}

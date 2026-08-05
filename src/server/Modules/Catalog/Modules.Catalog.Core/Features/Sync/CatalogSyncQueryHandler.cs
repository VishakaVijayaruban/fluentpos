// --------------------------------------------------------------------------------------------------
// <copyright file="CatalogSyncQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Products;
using FluentPOS.Shared.DTOs.Catalogs.StoreProducts;
using FluentPOS.Shared.DTOs.Catalogs.Sync;
using FluentPOS.Shared.DTOs.Catalogs.VatRates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Core.Features.Sync
{
    internal class CatalogSyncQueryHandler : IRequestHandler<GetCatalogSyncQuery, Result<CatalogSyncResponse>>
    {
        private readonly ICatalogDbContext _context;

        public CatalogSyncQueryHandler(ICatalogDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<CatalogSyncResponse>> Handle(GetCatalogSyncQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            // Captured before querying so changes landing mid-request are re-sent next pull
            // rather than missed. Duplicates are fine: clients upsert by id.
            var serverTime = DateTime.UtcNow;
            var since = request.Since ?? DateTime.MinValue;

            var response = new CatalogSyncResponse(serverTime)
            {
                Products = await _context.Products.AsNoTracking()
                    .Where(p => p.LastModifiedOn > since)
                    .Select(p => new GetProductsResponse(p.Id, p.Name, p.LocaleName, p.Barcode, p.BarcodeSymbology, p.Detail, p.BrandId, p.Brand.Name, p.CategoryId, p.Category.Name, p.Price, p.Cost, p.ImageUrl, p.VatRate.Rate, p.VatRateId, p.IsAgeRestricted, p.MinimumAge))
                    .ToListAsync(cancellationToken),

                // The store query filter scopes overlays to the caller's store automatically.
                StoreProducts = await _context.StoreProducts.AsNoTracking()
                    .Where(sp => sp.LastModifiedOn > since)
                    .Select(sp => new GetStoreProductsResponse(sp.Id, sp.StoreId, sp.ProductId, sp.Product.Name, sp.Price, sp.IsRanged, sp.ReorderPoint, sp.ReorderQuantity, sp.PreferredSupplierId))
                    .ToListAsync(cancellationToken),

                VatRates = await _context.VatRates.AsNoTracking()
                    .Where(v => v.LastModifiedOn > since)
                    .Select(v => new GetVatRatesResponse(v.Id, v.Name, v.Rate))
                    .ToListAsync(cancellationToken)
            };

            return await Result<CatalogSyncResponse>.SuccessAsync(response);
        }
    }
}

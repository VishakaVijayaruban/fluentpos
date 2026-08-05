// --------------------------------------------------------------------------------------------------
// <copyright file="StoreProductService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.DTOs.Catalogs.StoreProducts;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Infrastructure.Services
{
    public class StoreProductService : IStoreProductService
    {
        private readonly ICatalogDbContext _context;

        public StoreProductService(ICatalogDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReorderCandidate>> GetReorderCandidatesAsync()
        {
            return await _context.StoreProducts.AsNoTracking()
                .Where(sp => sp.IsRanged && sp.ReorderPoint != null)
                .Select(sp => new ReorderCandidate(
                    sp.StoreId,
                    sp.ProductId,
                    sp.Product.Name,
                    sp.Product.Cost,
                    sp.ReorderPoint.Value,
                    sp.ReorderQuantity ?? sp.ReorderPoint.Value,
                    sp.PreferredSupplierId))
                .ToListAsync();
        }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="StoreProductQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.StoreProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Core.Features.StoreProducts.Queries
{
    internal class StoreProductQueryHandler : IRequestHandler<GetStoreProductsQuery, Result<List<GetStoreProductsResponse>>>
    {
        private readonly ICatalogDbContext _context;

        public StoreProductQueryHandler(ICatalogDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetStoreProductsResponse>>> Handle(GetStoreProductsQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var queryable = _context.StoreProducts.AsNoTracking().AsQueryable();
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(sp => sp.StoreId == request.StoreId.Value);
            }

            var storeProducts = await queryable
                .OrderBy(sp => sp.Product.Name)
                .Select(sp => new GetStoreProductsResponse(sp.Id, sp.StoreId, sp.ProductId, sp.Product.Name, sp.Price, sp.IsRanged, sp.ReorderPoint, sp.ReorderQuantity))
                .ToListAsync(cancellationToken);

            return await Result<List<GetStoreProductsResponse>>.SuccessAsync(storeProducts);
        }
    }
}

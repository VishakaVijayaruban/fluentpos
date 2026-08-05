// --------------------------------------------------------------------------------------------------
// <copyright file="ProductService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Core.Exceptions;
using FluentPOS.Modules.Catalog.Core.Features.Products.Queries;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IMediator _mediator;
        private readonly ICatalogDbContext _context;

        public ProductService(IMediator mediator, ICatalogDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        public async Task<Result<GetProductByIdResponse>> GetDetailsAsync(Guid productId, Guid? storeId = null)
        {
            var response = await _mediator.Send(new GetProductByIdQuery(productId, false));
            if (!response.Succeeded || storeId == null)
            {
                return response;
            }

            var overlay = await _context.StoreProducts.AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.StoreId == storeId.Value && sp.ProductId == productId);
            if (overlay == null)
            {
                return response;
            }

            if (!overlay.IsRanged)
            {
                throw new CatalogException("Product is not ranged in this store.", HttpStatusCode.BadRequest);
            }

            if (overlay.Price.HasValue)
            {
                return await Result<GetProductByIdResponse>.SuccessAsync(response.Data with { Price = overlay.Price.Value });
            }

            return response;
        }
    }
}
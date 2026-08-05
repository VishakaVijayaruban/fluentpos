// --------------------------------------------------------------------------------------------------
// <copyright file="StoreProductCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Modules.Catalog.Core.Exceptions;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Catalog.Core.Features.StoreProducts.Commands
{
    internal class StoreProductCommandHandler :
        IRequestHandler<UpsertStoreProductCommand, Result<Guid>>,
        IRequestHandler<RemoveStoreProductCommand, Result<Guid>>
    {
        private readonly ICatalogDbContext _context;
        private readonly IStoreService _storeService;
        private readonly IStringLocalizer<StoreProductCommandHandler> _localizer;

        public StoreProductCommandHandler(
            ICatalogDbContext context,
            IStoreService storeService,
            IStringLocalizer<StoreProductCommandHandler> localizer)
        {
            _context = context;
            _storeService = storeService;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(UpsertStoreProductCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            if (!await _storeService.ExistsAsync(command.StoreId))
            {
                throw new CatalogException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            if (!await _context.Products.AnyAsync(p => p.Id == command.ProductId, cancellationToken))
            {
                throw new CatalogException(_localizer["Product Not Found!"], HttpStatusCode.NotFound);
            }

            var storeProduct = await _context.StoreProducts
                .FirstOrDefaultAsync(sp => sp.StoreId == command.StoreId && sp.ProductId == command.ProductId, cancellationToken);

            if (storeProduct == null)
            {
                storeProduct = new StoreProduct
                {
                    StoreId = command.StoreId,
                    ProductId = command.ProductId
                };
                await _context.StoreProducts.AddAsync(storeProduct, cancellationToken);
            }

            storeProduct.Price = command.Price;
            storeProduct.IsRanged = command.IsRanged;
            storeProduct.ReorderPoint = command.ReorderPoint;
            storeProduct.ReorderQuantity = command.ReorderQuantity;
            storeProduct.PreferredSupplierId = command.PreferredSupplierId;

            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(storeProduct.Id, _localizer["Store Product Saved"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RemoveStoreProductCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var storeProduct = await _context.StoreProducts.FirstOrDefaultAsync(sp => sp.Id == command.Id, cancellationToken);
            if (storeProduct == null)
            {
                throw new CatalogException(_localizer["Store Product Not Found!"], HttpStatusCode.NotFound);
            }

            _context.StoreProducts.Remove(storeProduct);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(storeProduct.Id, _localizer["Store Product Deleted"]);
        }
    }
}

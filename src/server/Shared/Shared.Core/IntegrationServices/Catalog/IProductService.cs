// --------------------------------------------------------------------------------------------------
// <copyright file="IProductService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Products;

namespace FluentPOS.Shared.Core.IntegrationServices.Catalog
{
    public interface IProductService
    {
        /// <summary>
        /// Product details with store-effective pricing: when a store is supplied and it
        /// has a price override for the product, that price is returned instead of the
        /// central one.
        /// </summary>
        Task<Result<GetProductByIdResponse>> GetDetailsAsync(Guid productId, Guid? storeId = null);
    }
}
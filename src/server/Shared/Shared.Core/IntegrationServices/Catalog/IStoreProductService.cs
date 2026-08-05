// --------------------------------------------------------------------------------------------------
// <copyright file="IStoreProductService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentPOS.Shared.DTOs.Catalogs.StoreProducts;

namespace FluentPOS.Shared.Core.IntegrationServices.Catalog
{
    /// <summary>
    /// Integration Services for per-store catalog settings.
    /// </summary>
    public interface IStoreProductService
    {
        /// <summary>
        /// All ranged store products that have a reorder point configured.
        /// </summary>
        Task<List<ReorderCandidate>> GetReorderCandidatesAsync();
    }
}

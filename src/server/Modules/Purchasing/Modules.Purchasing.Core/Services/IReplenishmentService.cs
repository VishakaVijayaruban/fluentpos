// --------------------------------------------------------------------------------------------------
// <copyright file="IReplenishmentService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace FluentPOS.Modules.Purchasing.Core.Services
{
    public interface IReplenishmentService
    {
        /// <summary>
        /// Scans per-store stock levels against reorder points and creates draft purchase
        /// orders grouped by store and preferred supplier. Skips products already on an
        /// open (draft/submitted) purchase order for that store.
        /// </summary>
        Task<ReplenishmentRunSummary> RunAsync();
    }

    public record ReplenishmentRunSummary(int OrdersCreated, int LinesAdded, int CandidatesScanned);
}

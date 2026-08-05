// --------------------------------------------------------------------------------------------------
// <copyright file="IOrganizationService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Core.IntegrationServices.Organization
{
    public record StoreOrganizationInfo(Guid OrganizationId, string OrganizationName, decimal RoyaltyRatePercent);

    /// <summary>
    /// Organization lookups for other modules (reporting projections, royalty accrual).
    /// </summary>
    public interface IOrganizationService
    {
        Task<StoreOrganizationInfo> GetStoreOrganizationAsync(Guid storeId);
    }
}

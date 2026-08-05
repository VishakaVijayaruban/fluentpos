// --------------------------------------------------------------------------------------------------
// <copyright file="IStoreService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Core.IntegrationServices.Organization
{
    /// <summary>
    /// Integration Services for the Organization Module.
    /// </summary>
    public interface IStoreService
    {
        Task<bool> ExistsAsync(Guid storeId);

        /// <summary>
        /// The store used when a request has no store context (e.g. head-office users on legacy clients).
        /// </summary>
        Task<Guid> GetDefaultStoreIdAsync();
    }
}

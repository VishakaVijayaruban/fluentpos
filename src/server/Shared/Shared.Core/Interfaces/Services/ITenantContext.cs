// --------------------------------------------------------------------------------------------------
// <copyright file="ITenantContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.Core.Interfaces.Services
{
    /// <summary>
    /// The store scope of the current request. Null means head-office scope: the caller
    /// is not restricted to a store and sees data across all stores.
    /// </summary>
    public interface ITenantContext
    {
        Guid? StoreId { get; }
    }
}

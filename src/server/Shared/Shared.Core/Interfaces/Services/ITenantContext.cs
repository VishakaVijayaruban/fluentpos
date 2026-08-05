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
    /// The tenancy scope of the current request. A store id restricts to one store; an
    /// organization id restricts to one franchisee's stores; both null means franchisor
    /// (platform) scope with visibility across everything.
    /// </summary>
    public interface ITenantContext
    {
        Guid? StoreId { get; }

        Guid? OrganizationId { get; }
    }
}

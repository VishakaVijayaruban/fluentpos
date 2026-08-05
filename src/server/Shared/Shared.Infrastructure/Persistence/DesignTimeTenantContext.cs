// --------------------------------------------------------------------------------------------------
// <copyright file="DesignTimeTenantContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Interfaces.Services;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Unscoped tenant context for design-time DbContext factories (migrations tooling).
    /// </summary>
    public class DesignTimeTenantContext : ITenantContext
    {
        public Guid? StoreId => null;
    }
}

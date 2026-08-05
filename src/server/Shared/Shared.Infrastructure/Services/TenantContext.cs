// --------------------------------------------------------------------------------------------------
// <copyright file="TenantContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace FluentPOS.Shared.Infrastructure.Services
{
    /// <summary>
    /// Resolves the current store scope from the authenticated user's token claims.
    /// Users without a store claim (head-office roles) operate unscoped.
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _accessor;

        public TenantContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid? StoreId
        {
            get
            {
                string claim = _accessor.HttpContext?.User?.FindFirst(ApplicationClaimTypes.Store)?.Value;
                return Guid.TryParse(claim, out var storeId) ? storeId : null;
            }
        }

        public Guid? OrganizationId
        {
            get
            {
                string claim = _accessor.HttpContext?.User?.FindFirst(ApplicationClaimTypes.Organization)?.Value;
                return Guid.TryParse(claim, out var organizationId) ? organizationId : null;
            }
        }
    }
}

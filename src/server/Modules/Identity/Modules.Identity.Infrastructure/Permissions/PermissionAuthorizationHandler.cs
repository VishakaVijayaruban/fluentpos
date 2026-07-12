// --------------------------------------------------------------------------------------------------
// <copyright file="PermissionAuthorizationHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FluentPOS.Modules.Identity.Infrastructure.Permissions
{
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionAuthorizationHandler()
        {
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                await Task.CompletedTask;
            }

            // Permission claims are embedded in our own signed JWT, so the token signature
            // (validated with issuer/audience checks) is what makes them trustworthy.
            var permissions = context.User.Claims.Where(x => x.Type == ApplicationClaimTypes.Permission &&
                                                                x.Value == requirement.Permission);
            if (permissions.Any())
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }
        }
    }
}
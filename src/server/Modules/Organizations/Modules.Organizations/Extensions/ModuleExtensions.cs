// --------------------------------------------------------------------------------------------------
// <copyright file="ModuleExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Organizations.Core.Extensions;
using FluentPOS.Modules.Organizations.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Organizations.Extensions
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddOrganizationsModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOrganizationsCore()
                .AddOrganizationsInfrastructure();
            return services;
        }

        public static IApplicationBuilder UseOrganizationsModule(this IApplicationBuilder app)
        {
            return app;
        }
    }
}

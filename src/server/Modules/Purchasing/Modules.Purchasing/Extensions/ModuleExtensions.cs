// --------------------------------------------------------------------------------------------------
// <copyright file="ModuleExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Purchasing.Core.Extensions;
using FluentPOS.Modules.Purchasing.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Purchasing.Extensions
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddPurchasingModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddPurchasingCore()
                .AddPurchasingInfrastructure();
            return services;
        }

        public static IApplicationBuilder UsePurchasingModule(this IApplicationBuilder app)
        {
            return app;
        }
    }
}

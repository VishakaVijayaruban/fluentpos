// --------------------------------------------------------------------------------------------------
// <copyright file="ModuleExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Reporting.Core.Extensions;
using FluentPOS.Modules.Reporting.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Reporting.Extensions
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddReportingModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddReportingCore()
                .AddReportingInfrastructure();
            return services;
        }

        public static IApplicationBuilder UseReportingModule(this IApplicationBuilder app)
        {
            return app;
        }
    }
}

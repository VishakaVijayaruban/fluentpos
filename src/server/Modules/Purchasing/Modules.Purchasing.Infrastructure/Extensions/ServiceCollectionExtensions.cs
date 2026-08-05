// --------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Reflection;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Services;
using FluentPOS.Modules.Purchasing.Infrastructure.Jobs;
using FluentPOS.Modules.Purchasing.Infrastructure.Persistence;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Purchasing.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPurchasingInfrastructure(this IServiceCollection services)
        {
            services
                .AddDatabaseContext<PurchasingDbContext>()
                .AddScoped<IPurchasingDbContext>(provider => provider.GetService<PurchasingDbContext>());
            services.AddTransient<IDatabaseSeeder, PurchasingDbSeeder>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddTransient<IReplenishmentService, ReplenishmentService>();
            services.AddTransient<ReplenishmentJob>();
            services.AddHostedService<ReplenishmentJobScheduler>();
            return services;
        }
    }
}

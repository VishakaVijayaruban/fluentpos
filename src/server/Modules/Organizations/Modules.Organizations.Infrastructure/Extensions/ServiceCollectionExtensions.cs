// --------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Reflection;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Infrastructure.Persistence;
using FluentPOS.Modules.Organizations.Infrastructure.Services;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Organizations.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrganizationsInfrastructure(this IServiceCollection services)
        {
            services
                .AddDatabaseContext<OrganizationDbContext>()
                .AddScoped<IOrganizationDbContext>(provider => provider.GetService<OrganizationDbContext>());
            services.AddTransient<IDatabaseSeeder, OrganizationDbSeeder>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddTransient<IStoreService, StoreService>();
            services.AddTransient<ITerminalService, TerminalService>();
            services.AddTransient<IOrganizationService, OrganizationService>();
            return services;
        }
    }
}

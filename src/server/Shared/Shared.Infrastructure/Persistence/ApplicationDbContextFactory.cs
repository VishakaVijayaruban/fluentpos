// --------------------------------------------------------------------------------------------------
// <copyright file="ApplicationDbContextFactory.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Shared.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    internal class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = DesignTimeConnectionString.Read();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(
                connectionString,
                e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            var persistenceSettings = Options.Create(new PersistenceSettings
            {
                UsePostgres = true,
                ConnectionStrings = new PersistenceSettings.PersistenceConnectionStrings
                {
                    Postgres = connectionString
                }
            });

            return new ApplicationDbContext(optionsBuilder.Options, persistenceSettings);
        }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="ReportingDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Reporting.Core.Abstractions;
using FluentPOS.Modules.Reporting.Core.Entities;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Reporting.Infrastructure.Persistence
{
    public sealed class ReportingDbContext : ModuleDbContext, IReportingDbContext
    {
        protected override string Schema => "Reporting";

        public ReportingDbContext(
            DbContextOptions<ReportingDbContext> options,
            IMediator mediator,
            IEventLogger eventLogger,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer json,
            ITenantContext tenant)
                : base(options, mediator, eventLogger, persistenceOptions, json, tenant)
        {
        }

        public DbSet<DailyStoreSales> DailyStoreSales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyStoreSales>(entity =>
            {
                entity.ToTable("DailyStoreSales");
                entity.HasIndex(r => new { r.StoreId, r.Date }).IsUnique();
                entity.HasIndex(r => r.OrganizationId);
            });
        }
    }
}

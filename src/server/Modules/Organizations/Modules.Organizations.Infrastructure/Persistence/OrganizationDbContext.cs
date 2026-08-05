// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Organizations.Infrastructure.Persistence
{
    public sealed class OrganizationDbContext : ModuleDbContext, IOrganizationDbContext
    {
        protected override string Schema => "Organization";

        public OrganizationDbContext(
            DbContextOptions<OrganizationDbContext> options,
            IMediator mediator,
            IEventLogger eventLogger,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer json,
            ITenantContext tenant)
                : base(options, mediator, eventLogger, persistenceOptions, json, tenant)
        {
        }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Store> Stores { get; set; }

        public DbSet<Terminal> Terminals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organizations");
                entity.Property(o => o.Name).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Stores");
                entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
                entity.HasOne(s => s.Organization)
                    .WithMany(o => o.Stores)
                    .HasForeignKey(s => s.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Terminal>(entity =>
            {
                entity.ToTable("Terminals");
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(t => t.Store)
                    .WithMany(s => s.Terminals)
                    .HasForeignKey(t => t.StoreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

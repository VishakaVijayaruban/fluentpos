// --------------------------------------------------------------------------------------------------
// <copyright file="SalesDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Interfaces.Services;

namespace FluentPOS.Modules.Sales.Infrastructure.Persistence
{
    public sealed class SalesDbContext : ModuleDbContext, ISalesDbContext
    {
        private readonly PersistenceSettings _persistenceOptions;
        private readonly IJsonSerializer _json;

        protected override string Schema => "Sales";

        public SalesDbContext(
            DbContextOptions<SalesDbContext> options,
            IMediator mediator,
            IEventLogger eventLogger,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer json,
            ITenantContext tenant)
                : base(options, mediator, eventLogger, persistenceOptions, json, tenant)
        {
            _persistenceOptions = persistenceOptions.Value;
            _json = json;
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<TillSession> TillSessions { get; set; }

        public DbSet<CashMovement> CashMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TillSession>(entity =>
            {
                entity.ToTable("TillSessions");
                entity.HasIndex(ts => new { ts.TerminalId, ts.Status });
            });

            modelBuilder.Entity<CashMovement>(entity =>
            {
                entity.ToTable("CashMovements");
                entity.HasIndex(cm => cm.TillSessionId);
            });
        }
    }
}
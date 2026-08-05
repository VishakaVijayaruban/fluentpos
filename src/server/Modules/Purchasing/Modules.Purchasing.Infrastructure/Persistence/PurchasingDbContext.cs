// --------------------------------------------------------------------------------------------------
// <copyright file="PurchasingDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Purchasing.Infrastructure.Persistence
{
    public sealed class PurchasingDbContext : ModuleDbContext, IPurchasingDbContext
    {
        protected override string Schema => "Purchasing";

        public PurchasingDbContext(
            DbContextOptions<PurchasingDbContext> options,
            IMediator mediator,
            IEventLogger eventLogger,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer json,
            ITenantContext tenant)
                : base(options, mediator, eventLogger, persistenceOptions, json, tenant)
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("PurchaseOrders");
                entity.HasIndex(po => new { po.StoreId, po.Status });
                entity.HasOne(po => po.Supplier)
                    .WithMany()
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(po => po.Items)
                    .WithOne()
                    .HasForeignKey(i => i.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.ToTable("PurchaseOrderItems");
                entity.Property(i => i.ProductName).HasMaxLength(150);
            });
        }
    }
}

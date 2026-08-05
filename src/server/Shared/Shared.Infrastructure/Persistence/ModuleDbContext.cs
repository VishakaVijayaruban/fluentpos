// --------------------------------------------------------------------------------------------------
// <copyright file="ModuleDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    public abstract class ModuleDbContext : DbContext, IModuleDbContext
    {
        private static readonly MethodInfo ConfigureStoreFilterMethod = typeof(ModuleDbContext)
            .GetMethod(nameof(ConfigureStoreFilter), BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IMediator _mediator;
        private readonly IEventLogger _eventLogger;
        private readonly PersistenceSettings _persistenceOptions;
        private readonly IJsonSerializer _json;
        private readonly ITenantContext _tenant;

        protected abstract string Schema { get; }

        // Referenced from query filter expressions; EF resolves it against the current context instance.
        protected Guid? CurrentStoreId => _tenant?.StoreId;

        protected ModuleDbContext(
            DbContextOptions options,
            IMediator mediator,
            IEventLogger eventLogger,
            IOptions<PersistenceSettings> persistenceOptions,
            IJsonSerializer json,
            ITenantContext tenant)
                : base(options)
        {
            _mediator = mediator;
            _eventLogger = eventLogger;
            _persistenceOptions = persistenceOptions.Value;
            _json = json;
            _tenant = tenant;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrWhiteSpace(Schema))
            {
                modelBuilder.HasDefaultSchema(Schema);
            }

            modelBuilder.Ignore<Event>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            modelBuilder.ApplyModuleConfiguration(_persistenceOptions);
            ApplyStoreQueryFilters(modelBuilder);
        }

        // Store isolation is the default: every IMustHaveStore entity is filtered to the
        // current tenant's store. Head-office requests (no store claim) are unfiltered.
        private void ApplyStoreQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IMustHaveStore).IsAssignableFrom(entityType.ClrType))
                {
                    ConfigureStoreFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        private void ConfigureStoreFilter<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, IMustHaveStore
        {
            // Note: no .Value on the nullable — EF evaluates it eagerly as a query parameter
            // even when the null check short-circuits, throwing for unscoped (HQ) requests.
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => CurrentStoreId == null || e.StoreId == CurrentStoreId);
        }

        private void StampStoreOnAddedEntries()
        {
            if (_tenant?.StoreId is not { } storeId)
            {
                return;
            }

            foreach (var entry in ChangeTracker.Entries<IMustHaveStore>())
            {
                if (entry.State == EntityState.Added && entry.Entity.StoreId == Guid.Empty)
                {
                    entry.Entity.StoreId = storeId;
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampStoreOnAddedEntries();
            var changes = OnBeforeSaveChanges();
            return await this.SaveChangeWithPublishEventsAsync(_eventLogger, _mediator, changes, _json, cancellationToken);
        }

        private List<(EntityEntry entityEntry, string oldValues, string newValues)> OnBeforeSaveChanges()
        {
            var result = new List<(EntityEntry entityEntry, string oldValues, string newValues)>();
            ChangeTracker.DetectChanges();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                var previousData = new Dictionary<string, object>();
                var currentData = new Dictionary<string, object>();
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    object originalValue = entry.GetDatabaseValues()?.GetValue<object>(propertyName);
                    switch (entry.State)
                    {
                        case EntityState.Unchanged:
                            break;
                        case EntityState.Added:
                            currentData[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            previousData[propertyName] = originalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified && originalValue?.Equals(property.CurrentValue) == false)
                            {
                                previousData[propertyName] = originalValue;
                                currentData[propertyName] = property.CurrentValue;
                            }

                            break;
                    }
                }

                string oldValues = previousData.Count == 0 ? null : _json.Serialize(previousData);
                string newValues = currentData.Count == 0 ? null : _json.Serialize(currentData);
                result.Add((entry, oldValues, newValues));
            }

            return result;
        }

        public override int SaveChanges()
        {
            StampStoreOnAddedEntries();
            var changes = OnBeforeSaveChanges();
            return this.SaveChangeWithPublishEvents(_eventLogger, _mediator, changes, _json);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return SaveChanges();
        }
    }
}
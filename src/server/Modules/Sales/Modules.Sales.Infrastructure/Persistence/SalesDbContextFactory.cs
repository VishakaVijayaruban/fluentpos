// --------------------------------------------------------------------------------------------------
// <copyright file="SalesDbContextFactory.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Domain;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces.Serialization;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Sales.Infrastructure.Persistence
{
    internal class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
    {
        public SalesDbContext CreateDbContext(string[] args)
        {
            var connectionString = DesignTimeConnectionString.Read();

            var optionsBuilder = new DbContextOptionsBuilder<SalesDbContext>();
            optionsBuilder.UseNpgsql(
                connectionString,
                e => e.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName));

            var persistenceSettings = Options.Create(new PersistenceSettings
            {
                UsePostgres = true,
                ConnectionStrings = new PersistenceSettings.PersistenceConnectionStrings
                {
                    Postgres = connectionString
                }
            });

            return new SalesDbContext(
                optionsBuilder.Options,
                new NullMediator(),
                new NullEventLogger(),
                persistenceSettings,
                new NullJsonSerializer());
        }

        private class NullMediator : IMediator
        {
            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
                => Task.FromResult<TResponse>(default);

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                where TRequest : IRequest
                => Task.CompletedTask;

            public Task<object> Send(object request, CancellationToken cancellationToken = default)
                => Task.FromResult<object>(null);

            public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                yield break;
            }

            public async IAsyncEnumerable<object> CreateStream(object request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                yield break;
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification
                => Task.CompletedTask;
        }

        private class NullEventLogger : IEventLogger
        {
            public Task SaveAsync<T>(T @event, (string oldValues, string newValues) changes)
                where T : Event
                => Task.CompletedTask;
        }

        private class NullJsonSerializer : IJsonSerializer
        {
            public string Serialize<T>(T obj, IJsonSerializerSettingsOptions settings = null)
                => null;

            public string Serialize<T>(T obj, Type type, IJsonSerializerSettingsOptions settings = null)
                => null;

            public T Deserialize<T>(string text, IJsonSerializerSettingsOptions settings = null)
                => default;
        }
    }
}

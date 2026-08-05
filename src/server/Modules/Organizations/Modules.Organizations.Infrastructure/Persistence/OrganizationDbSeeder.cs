// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationDbSeeder.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Interfaces.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace FluentPOS.Modules.Organizations.Infrastructure.Persistence
{
    public class OrganizationDbSeeder : IDatabaseSeeder
    {
        private static readonly Guid DefaultTerminalId = Guid.Parse("71000000-0000-4000-8000-000000000001");
        private static readonly Guid SecondTerminalId = Guid.Parse("71000000-0000-4000-8000-000000000002");

        private readonly ILogger<OrganizationDbSeeder> _logger;
        private readonly OrganizationDbContext _db;
        private readonly IStringLocalizer<OrganizationDbSeeder> _localizer;

        public OrganizationDbSeeder(
            ILogger<OrganizationDbSeeder> logger,
            OrganizationDbContext db,
            IStringLocalizer<OrganizationDbSeeder> localizer)
        {
            _logger = logger;
            _db = db;
            _localizer = localizer;
        }

        public void Initialize()
        {
            try
            {
                if (!_db.Organizations.Any())
                {
                    _db.Organizations.Add(new Organization
                    {
                        Id = OrganizationConstants.DefaultOrganizationId,
                        Name = "FluentPOS Retail",
                        Detail = "Default organization"
                    });

                    _db.Stores.AddRange(
                        new Store
                        {
                            Id = OrganizationConstants.DefaultStoreId,
                            OrganizationId = OrganizationConstants.DefaultOrganizationId,
                            Name = "Store One",
                            City = "Manchester",
                            IsDefault = true
                        },
                        new Store
                        {
                            Id = OrganizationConstants.SecondStoreId,
                            OrganizationId = OrganizationConstants.DefaultOrganizationId,
                            Name = "Store Two",
                            City = "Leeds"
                        });

                    _db.Terminals.AddRange(
                        new Terminal { Id = DefaultTerminalId, StoreId = OrganizationConstants.DefaultStoreId, Name = "Till 1" },
                        new Terminal { Id = SecondTerminalId, StoreId = OrganizationConstants.SecondStoreId, Name = "Till 1" });

                    _db.SaveChanges();
                    _logger.LogInformation(_localizer["Seeded Organization, Stores and Terminals."]);
                }
            }
            catch (Exception)
            {
                _logger.LogError(_localizer["An error occurred while seeding Organization data."]);
            }
        }
    }
}

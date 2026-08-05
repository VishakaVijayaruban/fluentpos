// --------------------------------------------------------------------------------------------------
// <copyright file="PurchasingDbSeeder.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Shared.Core.Interfaces.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace FluentPOS.Modules.Purchasing.Infrastructure.Persistence
{
    public class PurchasingDbSeeder : IDatabaseSeeder
    {
        public static readonly Guid DefaultSupplierId = Guid.Parse("9b000000-0000-4000-8000-000000000001");

        private readonly ILogger<PurchasingDbSeeder> _logger;
        private readonly PurchasingDbContext _db;
        private readonly IStringLocalizer<PurchasingDbSeeder> _localizer;

        public PurchasingDbSeeder(
            ILogger<PurchasingDbSeeder> logger,
            PurchasingDbContext db,
            IStringLocalizer<PurchasingDbSeeder> localizer)
        {
            _logger = logger;
            _db = db;
            _localizer = localizer;
        }

        public void Initialize()
        {
            try
            {
                if (!_db.Suppliers.Any())
                {
                    _db.Suppliers.Add(new Supplier
                    {
                        Id = DefaultSupplierId,
                        Name = "Booker Wholesale",
                        ContactName = "Trade Desk",
                        Email = "orders@booker.example",
                        Phone = "0800 000 000",
                        City = "Manchester"
                    });
                    _db.SaveChanges();
                    _logger.LogInformation(_localizer["Seeded Suppliers."]);
                }
            }
            catch (Exception)
            {
                _logger.LogError(_localizer["An error occurred while seeding Purchasing data."]);
            }
        }
    }
}

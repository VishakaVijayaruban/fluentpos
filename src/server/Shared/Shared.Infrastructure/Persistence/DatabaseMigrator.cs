// --------------------------------------------------------------------------------------------------
// <copyright file="DatabaseMigrator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Shared.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    public class DatabaseMigrator<TContext> : IDatabaseMigrator
        where TContext : DbContext
    {
        private readonly TContext _context;

        public DatabaseMigrator(TContext context)
        {
            _context = context;
        }

        public void Migrate()
        {
            _context.Database.Migrate();
        }
    }
}

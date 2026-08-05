// --------------------------------------------------------------------------------------------------
// <copyright file="IOrganizationDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Shared.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Core.Abstractions
{
    public interface IOrganizationDbContext : IDbContext
    {
        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Store> Stores { get; set; }

        public DbSet<Terminal> Terminals { get; set; }
    }
}

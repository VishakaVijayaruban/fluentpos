// --------------------------------------------------------------------------------------------------
// <copyright file="IReportingDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Reporting.Core.Entities;
using FluentPOS.Shared.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Reporting.Core.Abstractions
{
    public interface IReportingDbContext : IDbContext
    {
        public DbSet<DailyStoreSales> DailyStoreSales { get; set; }
    }
}

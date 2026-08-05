// --------------------------------------------------------------------------------------------------
// <copyright file="ReplenishmentJob.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Services;

namespace FluentPOS.Modules.Purchasing.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire entry point for the recurring auto-replenishment scan.
    /// </summary>
    public class ReplenishmentJob
    {
        private readonly IReplenishmentService _replenishmentService;

        public ReplenishmentJob(IReplenishmentService replenishmentService)
        {
            _replenishmentService = replenishmentService;
        }

        public async Task RunAsync()
        {
            await _replenishmentService.RunAsync();
        }
    }
}

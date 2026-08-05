// --------------------------------------------------------------------------------------------------
// <copyright file="ReplenishmentJobScheduler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace FluentPOS.Modules.Purchasing.Infrastructure.Jobs
{
    /// <summary>
    /// Registers the hourly auto-replenishment recurring job once the host (and Hangfire
    /// storage) is up.
    /// </summary>
    public class ReplenishmentJobScheduler : IHostedService
    {
        private readonly IRecurringJobManager _recurringJobs;

        public ReplenishmentJobScheduler(IRecurringJobManager recurringJobs)
        {
            _recurringJobs = recurringJobs;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _recurringJobs.AddOrUpdate<ReplenishmentJob>("purchasing-replenishment", job => job.RunAsync(), Cron.Hourly());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

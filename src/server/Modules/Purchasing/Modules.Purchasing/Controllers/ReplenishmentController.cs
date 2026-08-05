// --------------------------------------------------------------------------------------------------
// <copyright file="ReplenishmentController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Services;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Purchasing.Controllers
{
    [ApiVersion("1")]
    internal sealed class ReplenishmentController : BaseController
    {
        private readonly IReplenishmentService _replenishmentService;

        public ReplenishmentController(IReplenishmentService replenishmentService)
        {
            _replenishmentService = replenishmentService;
        }

        // The hourly Hangfire job runs the same scan; this lets back office trigger it on demand.
        [HttpPost("run")]
        [Authorize(Policy = Permissions.Replenishment.Run)]
        public async Task<IActionResult> RunAsync()
        {
            var summary = await _replenishmentService.RunAsync();
            return Ok(await Result<ReplenishmentRunSummary>.SuccessAsync(summary));
        }
    }
}

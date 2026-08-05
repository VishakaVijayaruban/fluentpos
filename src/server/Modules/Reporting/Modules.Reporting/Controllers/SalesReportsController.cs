// --------------------------------------------------------------------------------------------------
// <copyright file="SalesReportsController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Reporting.Core.Features.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Reporting.Controllers
{
    [ApiVersion("1")]
    internal sealed class SalesReportsController : BaseController
    {
        // Daily sales per store. Store staff see their store; franchisee managers their
        // organization; the franchisor sees everything.
        [HttpGet("daily")]
        [Authorize(Policy = Permissions.Reporting.View)]
        public async Task<IActionResult> GetDailyAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? storeId)
        {
            return Ok(await Mediator.Send(new GetDailySalesQuery { From = from, To = to, StoreId = storeId }));
        }

        // Royalty accrual grouped by organization (franchisor view; franchisees see their own).
        [HttpGet("royalties")]
        [Authorize(Policy = Permissions.Reporting.Royalties)]
        public async Task<IActionResult> GetRoyaltiesAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await Mediator.Send(new GetRoyaltiesQuery { From = from, To = to }));
        }
    }
}

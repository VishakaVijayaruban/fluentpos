// --------------------------------------------------------------------------------------------------
// <copyright file="SyncController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Features.Sync;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Catalog.Controllers
{
    [ApiVersion("1")]
    internal sealed class SyncController : BaseController
    {
        // Incremental catalog feed for POS nodes: pass the serverTime returned by the
        // previous pull as ?since= to receive only what changed.
        [HttpGet]
        [Authorize(Policy = Permissions.Products.ViewAll)]
        public async Task<IActionResult> GetAsync([FromQuery] DateTime? since)
        {
            return Ok(await Mediator.Send(new GetCatalogSyncQuery { Since = since }));
        }
    }
}

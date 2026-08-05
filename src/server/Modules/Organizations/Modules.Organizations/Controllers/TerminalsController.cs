// --------------------------------------------------------------------------------------------------
// <copyright file="TerminalsController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Features.Terminals.Commands;
using FluentPOS.Modules.Organizations.Core.Features.Terminals.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Organizations.Controllers
{
    [ApiVersion("1")]
    internal sealed class TerminalsController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Terminals.ViewAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? storeId)
        {
            return Ok(await Mediator.Send(new GetTerminalsQuery { StoreId = storeId }));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Terminals.Register)]
        public async Task<IActionResult> RegisterAsync(RegisterTerminalCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}

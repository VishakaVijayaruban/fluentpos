// --------------------------------------------------------------------------------------------------
// <copyright file="TillSessionsController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Features.TillSessions.Commands;
using FluentPOS.Modules.Sales.Core.Features.TillSessions.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Sales.Controllers
{
    [ApiVersion("1")]
    internal sealed class TillSessionsController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.TillSessions.ViewAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? storeId, [FromQuery] string status)
        {
            return Ok(await Mediator.Send(new GetTillSessionsQuery { StoreId = storeId, Status = status }));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.TillSessions.View)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await Mediator.Send(new GetTillSessionByIdQuery { Id = id }));
        }

        [HttpPost("open")]
        [Authorize(Policy = Permissions.TillSessions.Open)]
        public async Task<IActionResult> OpenAsync(OpenTillSessionCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/close")]
        [Authorize(Policy = Permissions.TillSessions.Close)]
        public async Task<IActionResult> CloseAsync(Guid id, [FromBody] CloseTillSessionCommand command)
        {
            command ??= new CloseTillSessionCommand();
            command.Id = id;
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/cash-movements")]
        [Authorize(Policy = Permissions.TillSessions.RecordCashMovement)]
        public async Task<IActionResult> RecordCashMovementAsync(Guid id, [FromBody] RecordCashMovementCommand command)
        {
            command ??= new RecordCashMovementCommand();
            command.TillSessionId = id;
            return Ok(await Mediator.Send(command));
        }
    }
}

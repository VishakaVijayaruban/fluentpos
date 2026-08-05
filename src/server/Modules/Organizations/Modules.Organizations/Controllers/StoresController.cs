// --------------------------------------------------------------------------------------------------
// <copyright file="StoresController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Features.Stores.Commands;
using FluentPOS.Modules.Organizations.Core.Features.Stores.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Organizations.Controllers
{
    [ApiVersion("1")]
    internal sealed class StoresController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Stores.ViewAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await Mediator.Send(new GetStoresQuery()));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Stores.View)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await Mediator.Send(new GetStoreByIdQuery { Id = id }));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Stores.Register)]
        public async Task<IActionResult> RegisterAsync(RegisterStoreCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        [Authorize(Policy = Permissions.Stores.Update)]
        public async Task<IActionResult> UpdateAsync(UpdateStoreCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Stores.Remove)]
        public async Task<IActionResult> RemoveAsync(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveStoreCommand { Id = id }));
        }
    }
}

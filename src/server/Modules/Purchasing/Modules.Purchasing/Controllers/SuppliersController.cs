// --------------------------------------------------------------------------------------------------
// <copyright file="SuppliersController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands;
using FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Purchasing.Controllers
{
    [ApiVersion("1")]
    internal sealed class SuppliersController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Suppliers.ViewAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await Mediator.Send(new GetSuppliersQuery()));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Suppliers.Register)]
        public async Task<IActionResult> RegisterAsync(RegisterSupplierCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        [Authorize(Policy = Permissions.Suppliers.Update)]
        public async Task<IActionResult> UpdateAsync(UpdateSupplierCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Suppliers.Remove)]
        public async Task<IActionResult> RemoveAsync(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveSupplierCommand { Id = id }));
        }
    }
}

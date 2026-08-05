// --------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrdersController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands;
using FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Purchasing.Controllers
{
    [ApiVersion("1")]
    internal sealed class PurchaseOrdersController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.PurchaseOrders.ViewAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? storeId, [FromQuery] string status)
        {
            return Ok(await Mediator.Send(new GetPurchaseOrdersQuery { StoreId = storeId, Status = status }));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.PurchaseOrders.View)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await Mediator.Send(new GetPurchaseOrderByIdQuery { Id = id }));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.PurchaseOrders.Register)]
        public async Task<IActionResult> CreateAsync(CreatePurchaseOrderCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/submit")]
        [Authorize(Policy = Permissions.PurchaseOrders.Update)]
        public async Task<IActionResult> SubmitAsync(Guid id, [FromBody] SubmitPurchaseOrderCommand command)
        {
            command ??= new SubmitPurchaseOrderCommand();
            command.Id = id;
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/receive")]
        [Authorize(Policy = Permissions.PurchaseOrders.Receive)]
        public async Task<IActionResult> ReceiveAsync(Guid id, [FromBody] ReceivePurchaseOrderCommand command)
        {
            command ??= new ReceivePurchaseOrderCommand();
            command.Id = id;
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Policy = Permissions.PurchaseOrders.Update)]
        public async Task<IActionResult> CancelAsync(Guid id)
        {
            return Ok(await Mediator.Send(new CancelPurchaseOrderCommand { Id = id }));
        }
    }
}

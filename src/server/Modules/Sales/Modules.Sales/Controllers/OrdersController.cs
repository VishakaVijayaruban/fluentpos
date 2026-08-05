// --------------------------------------------------------------------------------------------------
// <copyright file="OrdersController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Features.Sales.Commands;
using FluentPOS.Modules.Sales.Core.Features.Sales.Queries;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.DTOs.Sales.Orders;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Sales.Controllers
{
    [ApiVersion("1")]
    internal sealed class OrdersController : BaseController
    {

        [HttpGet]
        [Authorize(Policy = Permissions.Sales.ViewAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginatedSalesFilter filter)
        {
            var request = Mapper.Map<GetSalesQuery>(filter);
            var sales = await Mediator.Send(request);
            return Ok(sales);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Sales.View)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await Mediator.Send(new GetOrderByIdQuery {Id = id }));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Sales.Register)]
        public async Task<IActionResult> RegisterAsync(RegisterSaleCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        // Offline-capable POS checkout: the client owns the basket and supplies the sale id,
        // so queued sales can be replayed safely.
        [HttpPost("pos")]
        [Authorize(Policy = Permissions.Sales.Register)]
        public async Task<IActionResult> RegisterPosSaleAsync(RegisterPosSaleCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/refund")]
        [Authorize(Policy = Permissions.Sales.Refund)]
        public async Task<IActionResult> RefundAsync(Guid id, [FromBody] RefundSaleCommand command)
        {
            command ??= new RefundSaleCommand();
            command.OrderId = id;
            return Ok(await Mediator.Send(command));
        }


    }
}
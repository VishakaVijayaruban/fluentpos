// --------------------------------------------------------------------------------------------------
// <copyright file="StoreProductsController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Features.StoreProducts.Commands;
using FluentPOS.Modules.Catalog.Core.Features.StoreProducts.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Catalog.Controllers
{
    [ApiVersion("1")]
    internal sealed class StoreProductsController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.StoreProducts.ViewAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? storeId)
        {
            return Ok(await Mediator.Send(new GetStoreProductsQuery { StoreId = storeId }));
        }

        [HttpPost]
        [Authorize(Policy = Permissions.StoreProducts.Upsert)]
        public async Task<IActionResult> UpsertAsync(UpsertStoreProductCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.StoreProducts.Remove)]
        public async Task<IActionResult> RemoveAsync(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveStoreProductCommand { Id = id }));
        }
    }
}

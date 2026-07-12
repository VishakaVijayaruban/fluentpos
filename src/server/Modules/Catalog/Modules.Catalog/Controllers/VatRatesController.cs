// --------------------------------------------------------------------------------------------------
// <copyright file="VatRatesController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Features.VatRates.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Catalog.Controllers
{
    [ApiVersion("1")]
    internal sealed class VatRatesController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.VatRates.ViewAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await Mediator.Send(new GetVatRatesQuery()));
        }
    }
}

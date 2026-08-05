// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationsController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Features.Organizations.Queries;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Organizations.Controllers
{
    [ApiVersion("1")]
    internal sealed class OrganizationsController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Organizations.ViewAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await Mediator.Send(new GetOrganizationsQuery()));
        }
    }
}

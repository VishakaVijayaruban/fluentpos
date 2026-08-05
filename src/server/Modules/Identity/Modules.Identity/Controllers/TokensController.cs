// --------------------------------------------------------------------------------------------------
// <copyright file="TokensController.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FluentPOS.Shared.Core.Interfaces.Services.Identity;
using FluentPOS.Shared.DTOs.Identity.Tokens;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluentPOS.Modules.Identity.Controllers
{
    [ApiVersion("1")]
    internal sealed class TokensController : BaseController
    {
        private readonly ITokenService _tokenService;
        private readonly FluentPOS.Shared.Core.Interfaces.Services.Identity.ICurrentUser _currentUser;

        public TokensController(ITokenService tokenService, FluentPOS.Shared.Core.Interfaces.Services.Identity.ICurrentUser currentUser)
        {
            _tokenService = tokenService;
            _currentUser = currentUser;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetTokenAsync(TokenRequest request)
        {
            var token = await _tokenService.GetTokenAsync(request, GenerateIPAddress());
            return Ok(token);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult> RefreshAsync(RefreshTokenRequest request)
        {
            var response = await _tokenService.RefreshTokenAsync(request, GenerateIPAddress());
            return Ok(response);
        }

        // Operator PIN sign-in at a registered till; the token is scoped to the till's store.
        [HttpPost("pin")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosTokenAsync(PosTokenRequest request)
        {
            var token = await _tokenService.GetPosTokenAsync(request, GenerateIPAddress());
            return Ok(token);
        }

        // Sets the calling user's own POS PIN.
        [HttpPost("pin/setup")]
        [Authorize]
        public async Task<IActionResult> SetPosPinAsync(SetPosPinRequest request)
        {
            var result = await _tokenService.SetPosPinAsync(_currentUser.GetUserId().ToString(), request?.Pin);
            return Ok(result);
        }

        // ReSharper disable once InconsistentNaming
        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------
// <copyright file="TokenService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Exceptions;
using FluentPOS.Modules.Identity.Core.Settings;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Interfaces.Services.Identity;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Identity.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FluentPOS.Modules.Identity.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<FluentUser> _userManager;
        private readonly RoleManager<FluentRole> _roleManager;
        private readonly IStringLocalizer<TokenService> _localizer;
        private readonly SmsSettings _smsSettings;
        private readonly MailSettings _mailSettings;
        private readonly JwtSettings _config;
        private readonly IEventLogService _eventLog;
        private readonly ITerminalService _terminalService;
        private readonly IPasswordHasher<FluentUser> _pinHasher;

        public TokenService(
            UserManager<FluentUser> userManager,
            RoleManager<FluentRole> roleManager,
            IOptions<JwtSettings> config,
            IStringLocalizer<TokenService> localizer,
            IOptions<SmsSettings> smsSettings,
            IOptions<MailSettings> mailSettings,
            IEventLogService eventLog,
            ITerminalService terminalService,
            IPasswordHasher<FluentUser> pinHasher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            _smsSettings = smsSettings.Value;
            _mailSettings = mailSettings.Value;
            _config = config.Value;
            _eventLog = eventLog;
            _terminalService = terminalService;
            _pinHasher = pinHasher;
        }

        public async Task<IResult<TokenResponse>> GetTokenAsync(TokenRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new IdentityException(_localizer["User Not Found."], statusCode: HttpStatusCode.Unauthorized);
            }

            if (!user.IsActive)
            {
                throw new IdentityException(_localizer["User Not Active. Please contact the administrator."], statusCode: HttpStatusCode.Unauthorized);
            }

            if (_mailSettings.EnableVerification && !user.EmailConfirmed)
            {
                throw new IdentityException(_localizer["E-Mail not confirmed."], statusCode: HttpStatusCode.Unauthorized);
            }

            if (_smsSettings.EnableVerification && !user.PhoneNumberConfirmed)
            {
                throw new IdentityException(_localizer["Phone Number not confirmed."], statusCode: HttpStatusCode.Unauthorized);
            }

            bool passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new IdentityException(_localizer["Invalid Credentials."], statusCode: HttpStatusCode.Unauthorized);
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config.RefreshTokenExpirationInDays);
            await _userManager.UpdateAsync(user);
            string token = await GenerateJwtAsync(user, ipAddress);
            var response = new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
            await _eventLog.LogCustomEventAsync(new() { Description = $"Generated Tokens for {user.Email}.", Email = user.Email });
            return await Result<TokenResponse>.SuccessAsync(response);
        }

        public async Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            if (request is null)
            {
                throw new IdentityException(_localizer["Invalid Client Token."], statusCode: HttpStatusCode.Unauthorized);
            }

            var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
            string userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                throw new IdentityException(_localizer["User Not Found."], statusCode: HttpStatusCode.NotFound);
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new IdentityException(_localizer["Invalid Client Token."], statusCode: HttpStatusCode.Unauthorized);
            }

            string token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user, ipAddress));
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config.RefreshTokenExpirationInDays);
            await _userManager.UpdateAsync(user);
            var response = new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
            return await Result<TokenResponse>.SuccessAsync(response);
        }

        public async Task<IResult<TokenResponse>> GetPosTokenAsync(PosTokenRequest request, string ipAddress)
        {
            // The device key proves the request comes from a registered till.
            Guid? terminalStoreId = await _terminalService.ValidateDeviceKeyAsync(request.TerminalId, request.DeviceKey);
            if (terminalStoreId == null)
            {
                throw new IdentityException(_localizer["Invalid terminal or device key."], statusCode: HttpStatusCode.Unauthorized);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                throw new IdentityException(_localizer["Invalid Credentials."], statusCode: HttpStatusCode.Unauthorized);
            }

            if (string.IsNullOrEmpty(user.PosPinHash)
                || _pinHasher.VerifyHashedPassword(user, user.PosPinHash, request.Pin) == PasswordVerificationResult.Failed)
            {
                throw new IdentityException(_localizer["Invalid Credentials."], statusCode: HttpStatusCode.Unauthorized);
            }

            // Store-scoped operators may only sign in at their own store's tills.
            if (user.StoreId.HasValue && user.StoreId.Value != terminalStoreId.Value)
            {
                throw new IdentityException(_localizer["You cannot sign in at a till belonging to another store."], statusCode: HttpStatusCode.Unauthorized);
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config.RefreshTokenExpirationInDays);
            await _userManager.UpdateAsync(user);

            // POS tokens are always scoped to the terminal's store - even for head-office users.
            string token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user, ipAddress, terminalStoreId));
            var response = new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
            await _eventLog.LogCustomEventAsync(new() { Description = $"POS sign-in for {user.Email} at terminal {request.TerminalId}.", Email = user.Email });
            return await Result<TokenResponse>.SuccessAsync(response);
        }

        public async Task<IResult<string>> SetPosPinAsync(string userId, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4 || pin.Length > 8 || !pin.All(char.IsDigit))
            {
                throw new IdentityException(_localizer["The PIN must be 4 to 8 digits."], statusCode: HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new IdentityException(_localizer["User Not Found."], statusCode: HttpStatusCode.NotFound);
            }

            user.PosPinHash = _pinHasher.HashPassword(user, pin);
            await _userManager.UpdateAsync(user);
            return await Result<string>.SuccessAsync(_localizer["POS PIN updated."].ToString());
        }

        private async Task<string> GenerateJwtAsync(FluentUser user, string ipAddress)
        {
            return GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user, ipAddress));
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(FluentUser user, string ipAddress, Guid? storeOverride = null)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();
            foreach (string role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
                var thisRole = await _roleManager.FindByNameAsync(role);
                var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
                permissionClaims.AddRange(allPermissionsForThisRoles);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new("fullName", $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new("ipAddress", ipAddress)
            };

            Guid? storeClaim = storeOverride ?? user.StoreId;
            if (storeClaim.HasValue)
            {
                claims.Add(new Claim(ApplicationClaimTypes.Store, storeClaim.Value.ToString()));
            }

            if (user.OrganizationId.HasValue)
            {
                claims.Add(new Claim(ApplicationClaimTypes.Organization, user.OrganizationId.Value.ToString()));
            }

            return claims
                .Union(userClaims)
                .Union(roleClaims)
                .Union(permissionClaims);
        }

        private string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
               issuer: _config.Issuer,
               audience: _config.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(_config.TokenExpirationInMinutes),
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key)),
                ValidateIssuer = true,
                ValidIssuer = _config.Issuer,
                ValidateAudience = true,
                ValidAudience = _config.Audience,

                // The token being refreshed is expected to be expired; the refresh token guards the flow.
                ValidateLifetime = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new IdentityException(_localizer["Invalid Token."], statusCode: HttpStatusCode.Unauthorized);
            }

            return principal;
        }

        private SigningCredentials GetSigningCredentials()
        {
            byte[] secret = Encoding.UTF8.GetBytes(_config.Key);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }
    }
}
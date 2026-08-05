// --------------------------------------------------------------------------------------------------
// <copyright file="TerminalService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Infrastructure.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly IOrganizationDbContext _context;

        public TerminalService(IOrganizationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid?> ValidateDeviceKeyAsync(Guid terminalId, string deviceKey)
        {
            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                return null;
            }

            var terminal = await _context.Terminals.AsNoTracking()
                .Where(t => t.Id == terminalId && t.IsActive && t.DeviceKeyHash != null)
                .Select(t => new { t.DeviceKeyHash, t.StoreId })
                .FirstOrDefaultAsync();
            if (terminal == null)
            {
                return null;
            }

            string providedHash;
            try
            {
                providedHash = Convert.ToBase64String(SHA256.HashData(Convert.FromBase64String(deviceKey)));
            }
            catch (FormatException)
            {
                return null;
            }

            return string.Equals(providedHash, terminal.DeviceKeyHash, StringComparison.Ordinal)
                ? terminal.StoreId
                : null;
        }
    }
}

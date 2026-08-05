// --------------------------------------------------------------------------------------------------
// <copyright file="TerminalCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Modules.Organizations.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Terminals.Commands
{
    internal class TerminalCommandHandler :
        IRequestHandler<RegisterTerminalCommand, Result<Guid>>,
        IRequestHandler<RegisterTerminalDeviceCommand, Result<string>>
    {
        private readonly IOrganizationDbContext _context;
        private readonly IStringLocalizer<TerminalCommandHandler> _localizer;

        public TerminalCommandHandler(
            IOrganizationDbContext context,
            IStringLocalizer<TerminalCommandHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterTerminalCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            if (!await _context.Stores.AnyAsync(s => s.Id == command.StoreId, cancellationToken))
            {
                throw new OrganizationException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            var terminal = new Terminal
            {
                StoreId = command.StoreId,
                Name = command.Name
            };

            await _context.Terminals.AddAsync(terminal, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(terminal.Id, _localizer["Terminal Saved"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<string>> Handle(RegisterTerminalDeviceCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var terminal = await _context.Terminals.FirstOrDefaultAsync(t => t.Id == command.TerminalId, cancellationToken);
            if (terminal == null)
            {
                throw new OrganizationException(_localizer["Terminal Not Found!"], HttpStatusCode.NotFound);
            }

            byte[] keyBytes = RandomNumberGenerator.GetBytes(32);
            string deviceKey = Convert.ToBase64String(keyBytes);
            terminal.DeviceKeyHash = Convert.ToBase64String(SHA256.HashData(Convert.FromBase64String(deviceKey)));
            await _context.SaveChangesAsync(cancellationToken);

            // The plaintext key is shown exactly once; store it on the device.
            return await Result<string>.SuccessAsync(deviceKey, _localizer["Terminal device registered. Store this key securely - it will not be shown again."]);
        }
    }
}

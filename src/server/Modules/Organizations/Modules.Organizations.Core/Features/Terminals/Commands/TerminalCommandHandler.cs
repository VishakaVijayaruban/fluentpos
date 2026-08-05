// --------------------------------------------------------------------------------------------------
// <copyright file="TerminalCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
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
    internal class TerminalCommandHandler : IRequestHandler<RegisterTerminalCommand, Result<Guid>>
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
    }
}

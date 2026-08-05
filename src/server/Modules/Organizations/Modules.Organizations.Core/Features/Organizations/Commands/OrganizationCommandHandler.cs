// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Organizations.Commands
{
    internal class OrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, Result<Guid>>
    {
        private readonly IOrganizationDbContext _context;
        private readonly IStringLocalizer<OrganizationCommandHandler> _localizer;

        public OrganizationCommandHandler(
            IOrganizationDbContext context,
            IStringLocalizer<OrganizationCommandHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterOrganizationCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var organization = new Organization
            {
                Name = command.Name,
                Detail = command.Detail,
                RoyaltyRatePercent = command.RoyaltyRatePercent
            };

            await _context.Organizations.AddAsync(organization, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(organization.Id, _localizer["Organization Saved"]);
        }
    }
}

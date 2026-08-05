// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Organizations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Core.Features.Organizations.Queries
{
    internal class OrganizationQueryHandler : IRequestHandler<GetOrganizationsQuery, Result<List<GetOrganizationsResponse>>>
    {
        private readonly IOrganizationDbContext _context;

        public OrganizationQueryHandler(IOrganizationDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetOrganizationsResponse>>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var organizations = await _context.Organizations.AsNoTracking()
                .OrderBy(o => o.Name)
                .Select(o => new GetOrganizationsResponse(o.Id, o.Name, o.Detail))
                .ToListAsync(cancellationToken);

            return await Result<List<GetOrganizationsResponse>>.SuccessAsync(organizations);
        }
    }
}

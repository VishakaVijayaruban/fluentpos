// --------------------------------------------------------------------------------------------------
// <copyright file="TerminalQueryHandler.cs" company="FluentPOS">
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
using FluentPOS.Shared.DTOs.Organizations.Terminals;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Core.Features.Terminals.Queries
{
    internal class TerminalQueryHandler : IRequestHandler<GetTerminalsQuery, Result<List<GetTerminalsResponse>>>
    {
        private readonly IOrganizationDbContext _context;

        public TerminalQueryHandler(IOrganizationDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetTerminalsResponse>>> Handle(GetTerminalsQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var queryable = _context.Terminals.AsNoTracking().AsQueryable();
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(t => t.StoreId == request.StoreId.Value);
            }

            var terminals = await queryable
                .OrderBy(t => t.Name)
                .Select(t => new GetTerminalsResponse(t.Id, t.StoreId, t.Name, t.IsActive))
                .ToListAsync(cancellationToken);

            return await Result<List<GetTerminalsResponse>>.SuccessAsync(terminals);
        }
    }
}

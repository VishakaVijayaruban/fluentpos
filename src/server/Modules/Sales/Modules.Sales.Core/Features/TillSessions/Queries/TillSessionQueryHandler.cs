// --------------------------------------------------------------------------------------------------
// <copyright file="TillSessionQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Modules.Sales.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Sales.Tills;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Queries
{
    internal class TillSessionQueryHandler :
        IRequestHandler<GetTillSessionsQuery, Result<List<GetTillSessionsResponse>>>,
        IRequestHandler<GetTillSessionByIdQuery, Result<GetTillSessionsResponse>>
    {
        private readonly ISalesDbContext _context;
        private readonly IStringLocalizer<TillSessionQueryHandler> _localizer;

        public TillSessionQueryHandler(
            ISalesDbContext context,
            IStringLocalizer<TillSessionQueryHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetTillSessionsResponse>>> Handle(GetTillSessionsQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var queryable = _context.TillSessions.AsNoTracking().AsQueryable();
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(ts => ts.StoreId == request.StoreId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && System.Enum.TryParse<TillSessionStatus>(request.Status, true, out var status))
            {
                queryable = queryable.Where(ts => ts.Status == status);
            }

            var sessions = await queryable
                .OrderByDescending(ts => ts.OpenedAt)
                .ToListAsync(cancellationToken);

            return await Result<List<GetTillSessionsResponse>>.SuccessAsync(sessions.Select(Map).ToList());
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<GetTillSessionsResponse>> Handle(GetTillSessionByIdQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var session = await _context.TillSessions.AsNoTracking().FirstOrDefaultAsync(ts => ts.Id == request.Id, cancellationToken);
            if (session == null)
            {
                throw new SalesException(_localizer["Till Session Not Found!"], HttpStatusCode.NotFound);
            }

            var response = Map(session);
            response.CashPaymentsTotal = await _context.Transactions
                .Where(t => t.TillSessionId == session.Id && t.PaymentType == PaymentType.Cash)
                .SumAsync(t => t.Amount, cancellationToken);
            response.PayInsTotal = await _context.CashMovements
                .Where(cm => cm.TillSessionId == session.Id && cm.Kind == CashMovementKind.PayIn)
                .SumAsync(cm => cm.Amount, cancellationToken);
            response.PayOutsTotal = await _context.CashMovements
                .Where(cm => cm.TillSessionId == session.Id && cm.Kind == CashMovementKind.PayOut)
                .SumAsync(cm => cm.Amount, cancellationToken);
            response.RunningExpectedCash = session.OpeningFloat + response.CashPaymentsTotal + response.PayInsTotal - response.PayOutsTotal;

            return await Result<GetTillSessionsResponse>.SuccessAsync(response);
        }

        private static GetTillSessionsResponse Map(TillSession session)
        {
            return new GetTillSessionsResponse(session.Id, session.StoreId, session.TerminalId, session.Status.ToString(), session.OpenedByUserId, session.OpenedAt, session.OpeningFloat, session.ClosedByUserId, session.ClosedAt, session.CountedCash, session.ExpectedCash, session.Variance, session.Notes);
        }
    }
}

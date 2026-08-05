// --------------------------------------------------------------------------------------------------
// <copyright file="TillSessionCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Modules.Sales.Core.Exceptions;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Interfaces.Services.Identity;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Commands
{
    internal class TillSessionCommandHandler :
        IRequestHandler<OpenTillSessionCommand, Result<Guid>>,
        IRequestHandler<CloseTillSessionCommand, Result<Guid>>,
        IRequestHandler<RecordCashMovementCommand, Result<Guid>>
    {
        private readonly ISalesDbContext _context;
        private readonly ITenantContext _tenant;
        private readonly IStoreService _storeService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<TillSessionCommandHandler> _localizer;

        public TillSessionCommandHandler(
            ISalesDbContext context,
            ITenantContext tenant,
            IStoreService storeService,
            ICurrentUser currentUser,
            IStringLocalizer<TillSessionCommandHandler> localizer)
        {
            _context = context;
            _tenant = tenant;
            _storeService = storeService;
            _currentUser = currentUser;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(OpenTillSessionCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            Guid storeId = command.StoreId ?? _tenant.StoreId ?? await _storeService.GetDefaultStoreIdAsync();
            if (_tenant.StoreId.HasValue && storeId != _tenant.StoreId.Value)
            {
                throw new SalesException(_localizer["You cannot open a till session for another store."], HttpStatusCode.Forbidden);
            }

            if (!await _storeService.ExistsAsync(storeId))
            {
                throw new SalesException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            bool alreadyOpen = await _context.TillSessions
                .AnyAsync(ts => ts.TerminalId == command.TerminalId && ts.Status == TillSessionStatus.Open, cancellationToken);
            if (alreadyOpen)
            {
                throw new SalesException(_localizer["This terminal already has an open till session."], HttpStatusCode.BadRequest);
            }

            var session = TillSession.Open(storeId, command.TerminalId, _currentUser.GetUserId(), command.OpeningFloat);
            await _context.TillSessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(session.Id, _localizer["Till Session Opened"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(CloseTillSessionCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var session = await _context.TillSessions.FirstOrDefaultAsync(ts => ts.Id == command.Id, cancellationToken);
            if (session == null)
            {
                throw new SalesException(_localizer["Till Session Not Found!"], HttpStatusCode.NotFound);
            }

            decimal expectedCash = await ComputeExpectedCashAsync(session, cancellationToken);

            try
            {
                session.Close(command.CountedCash, expectedCash, _currentUser.GetUserId(), command.Notes);
            }
            catch (InvalidOperationException ex)
            {
                throw new SalesException(ex.Message, HttpStatusCode.BadRequest);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(session.Id, string.Format(_localizer["Till Session Closed. Variance: {0}"], session.Variance));
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RecordCashMovementCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var session = await _context.TillSessions.FirstOrDefaultAsync(ts => ts.Id == command.TillSessionId, cancellationToken);
            if (session == null)
            {
                throw new SalesException(_localizer["Till Session Not Found!"], HttpStatusCode.NotFound);
            }

            if (session.Status != TillSessionStatus.Open)
            {
                throw new SalesException(_localizer["Cash movements can only be recorded on an open till session."], HttpStatusCode.BadRequest);
            }

            var movement = CashMovement.Record(session.StoreId, session.Id, command.Kind, command.Amount, command.Reason);
            await _context.CashMovements.AddAsync(movement, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(movement.Id, _localizer["Cash Movement Recorded"]);
        }

        // Float + cash takings (refunds are negative) + pay-ins - pay-outs.
        private async Task<decimal> ComputeExpectedCashAsync(TillSession session, CancellationToken cancellationToken)
        {
            decimal cashPayments = await _context.Transactions
                .Where(t => t.TillSessionId == session.Id && t.PaymentType == PaymentType.Cash)
                .SumAsync(t => t.Amount, cancellationToken);

            decimal payIns = await _context.CashMovements
                .Where(cm => cm.TillSessionId == session.Id && cm.Kind == CashMovementKind.PayIn)
                .SumAsync(cm => cm.Amount, cancellationToken);

            decimal payOuts = await _context.CashMovements
                .Where(cm => cm.TillSessionId == session.Id && cm.Kind == CashMovementKind.PayOut)
                .SumAsync(cm => cm.Amount, cancellationToken);

            return session.OpeningFloat + cashPayments + payIns - payOuts;
        }
    }
}

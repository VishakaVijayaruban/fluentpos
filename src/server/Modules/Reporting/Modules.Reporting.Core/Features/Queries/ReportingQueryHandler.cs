// --------------------------------------------------------------------------------------------------
// <copyright file="ReportingQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Reporting.Core.Abstractions;
using FluentPOS.Modules.Reporting.Core.Entities;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Reporting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Reporting.Core.Features.Queries
{
    internal class ReportingQueryHandler :
        IRequestHandler<GetDailySalesQuery, Result<List<GetDailySalesResponse>>>,
        IRequestHandler<GetRoyaltiesQuery, Result<List<GetRoyaltySummaryResponse>>>
    {
        private readonly IReportingDbContext _context;
        private readonly ITenantContext _tenant;

        public ReportingQueryHandler(IReportingDbContext context, ITenantContext tenant)
        {
            _context = context;
            _tenant = tenant;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetDailySalesResponse>>> Handle(GetDailySalesQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var rows = await ScopedRows(request.StoreId)
                .Where(r => (request.From == null || r.Date >= request.From.Value.Date)
                         && (request.To == null || r.Date <= request.To.Value.Date))
                .OrderByDescending(r => r.Date).ThenBy(r => r.OrganizationName)
                .Select(r => new GetDailySalesResponse(r.StoreId, r.OrganizationId, r.OrganizationName, r.Date, r.OrdersCount, r.GrossSales, r.TaxTotal, r.RefundsTotal, r.NetSales, r.RoyaltyRatePercent, r.RoyaltyAmount))
                .ToListAsync(cancellationToken);

            return await Result<List<GetDailySalesResponse>>.SuccessAsync(rows);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetRoyaltySummaryResponse>>> Handle(GetRoyaltiesQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            // Project to an anonymous type first: EF cannot translate a GroupBy straight
            // into a record constructor.
            var grouped = await ScopedRows(null)
                .Where(r => (request.From == null || r.Date >= request.From.Value.Date)
                         && (request.To == null || r.Date <= request.To.Value.Date))
                .GroupBy(r => new { r.OrganizationId, r.OrganizationName, r.RoyaltyRatePercent })
                .Select(g => new
                {
                    g.Key.OrganizationId,
                    g.Key.OrganizationName,
                    g.Key.RoyaltyRatePercent,
                    NetSales = g.Sum(r => r.NetSales),
                    RoyaltyAmount = g.Sum(r => r.RoyaltyAmount)
                })
                .OrderBy(g => g.OrganizationName)
                .ToListAsync(cancellationToken);

            var rows = grouped
                .Select(g => new GetRoyaltySummaryResponse(g.OrganizationId, g.OrganizationName, g.RoyaltyRatePercent, g.NetSales, g.RoyaltyAmount))
                .ToList();

            return await Result<List<GetRoyaltySummaryResponse>>.SuccessAsync(rows);
        }

        // Store scoping comes from the global query filter; organization scoping is applied here
        // so a franchisee manager only ever sees their own organization's figures.
        private IQueryable<DailyStoreSales> ScopedRows(System.Guid? storeId)
        {
            var queryable = _context.DailyStoreSales.AsNoTracking().AsQueryable();
            if (_tenant.OrganizationId.HasValue)
            {
                queryable = queryable.Where(r => r.OrganizationId == _tenant.OrganizationId.Value);
            }

            if (storeId.HasValue)
            {
                queryable = queryable.Where(r => r.StoreId == storeId.Value);
            }

            return queryable;
        }
    }
}

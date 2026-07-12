// --------------------------------------------------------------------------------------------------
// <copyright file="VatRateQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.VatRates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Core.Features.VatRates.Queries
{
    internal class VatRateQueryHandler : IRequestHandler<GetVatRatesQuery, Result<List<GetVatRatesResponse>>>
    {
        private readonly ICatalogDbContext _context;

        public VatRateQueryHandler(ICatalogDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetVatRatesResponse>>> Handle(GetVatRatesQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var vatRates = await _context.VatRates.AsNoTracking()
                .OrderBy(v => v.Rate)
                .Select(v => new GetVatRatesResponse(v.Id, v.Name, v.Rate))
                .ToListAsync(cancellationToken);

            return await Result<List<GetVatRatesResponse>>.SuccessAsync(vatRates);
        }
    }
}

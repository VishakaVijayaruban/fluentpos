// --------------------------------------------------------------------------------------------------
// <copyright file="SupplierQueryHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Purchasing.Suppliers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Queries
{
    internal class SupplierQueryHandler : IRequestHandler<GetSuppliersQuery, Result<List<GetSuppliersResponse>>>
    {
        private readonly IPurchasingDbContext _context;

        public SupplierQueryHandler(IPurchasingDbContext context)
        {
            _context = context;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetSuppliersResponse>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var suppliers = await _context.Suppliers.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new GetSuppliersResponse(s.Id, s.Name, s.ContactName, s.Email, s.Phone, s.AddressLine, s.City, s.Postcode, s.IsActive))
                .ToListAsync(cancellationToken);

            return await Result<List<GetSuppliersResponse>>.SuccessAsync(suppliers);
        }
    }
}

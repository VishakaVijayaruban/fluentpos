// --------------------------------------------------------------------------------------------------
// <copyright file="StoreQueryHandler.cs" company="FluentPOS">
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
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Organizations.Stores;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Stores.Queries
{
    internal class StoreQueryHandler :
        IRequestHandler<GetStoresQuery, Result<List<GetStoresResponse>>>,
        IRequestHandler<GetStoreByIdQuery, Result<GetStoresResponse>>
    {
        private readonly IOrganizationDbContext _context;
        private readonly IStringLocalizer<StoreQueryHandler> _localizer;

        public StoreQueryHandler(
            IOrganizationDbContext context,
            IStringLocalizer<StoreQueryHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<List<GetStoresResponse>>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var stores = await _context.Stores.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new GetStoresResponse(s.Id, s.OrganizationId, s.Name, s.AddressLine, s.City, s.Postcode, s.Phone, s.IsDefault, s.IsActive))
                .ToListAsync(cancellationToken);

            return await Result<List<GetStoresResponse>>.SuccessAsync(stores);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<GetStoresResponse>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var store = await _context.Stores.AsNoTracking()
                .Where(s => s.Id == request.Id)
                .Select(s => new GetStoresResponse(s.Id, s.OrganizationId, s.Name, s.AddressLine, s.City, s.Postcode, s.Phone, s.IsDefault, s.IsActive))
                .FirstOrDefaultAsync(cancellationToken);

            if (store == null)
            {
                throw new OrganizationException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            return await Result<GetStoresResponse>.SuccessAsync(store);
        }
    }
}

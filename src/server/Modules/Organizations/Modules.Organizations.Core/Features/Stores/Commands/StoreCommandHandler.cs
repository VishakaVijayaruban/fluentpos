// --------------------------------------------------------------------------------------------------
// <copyright file="StoreCommandHandler.cs" company="FluentPOS">
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
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Entities;
using FluentPOS.Modules.Organizations.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Stores.Commands
{
    internal class StoreCommandHandler :
        IRequestHandler<RegisterStoreCommand, Result<Guid>>,
        IRequestHandler<UpdateStoreCommand, Result<Guid>>,
        IRequestHandler<RemoveStoreCommand, Result<Guid>>
    {
        private readonly IOrganizationDbContext _context;
        private readonly IStringLocalizer<StoreCommandHandler> _localizer;

        public StoreCommandHandler(
            IOrganizationDbContext context,
            IStringLocalizer<StoreCommandHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterStoreCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            Guid organizationId = command.OrganizationId
                ?? await _context.Organizations.AsNoTracking().OrderBy(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync(cancellationToken);
            if (organizationId == Guid.Empty || !await _context.Organizations.AnyAsync(o => o.Id == organizationId, cancellationToken))
            {
                throw new OrganizationException(_localizer["Organization Not Found!"], HttpStatusCode.NotFound);
            }

            var store = new Store
            {
                OrganizationId = organizationId,
                Name = command.Name,
                AddressLine = command.AddressLine,
                City = command.City,
                Postcode = command.Postcode,
                Phone = command.Phone,
                IsDefault = !await _context.Stores.AnyAsync(cancellationToken)
            };

            await _context.Stores.AddAsync(store, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(store.Id, _localizer["Store Saved"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(UpdateStoreCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (store == null)
            {
                throw new OrganizationException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            store.Name = command.Name;
            store.AddressLine = command.AddressLine;
            store.City = command.City;
            store.Postcode = command.Postcode;
            store.Phone = command.Phone;
            store.IsActive = command.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(store.Id, _localizer["Store Updated"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RemoveStoreCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (store == null)
            {
                throw new OrganizationException(_localizer["Store Not Found!"], HttpStatusCode.NotFound);
            }

            if (store.IsDefault)
            {
                throw new OrganizationException(_localizer["The default store cannot be removed."], HttpStatusCode.BadRequest);
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(store.Id, _localizer["Store Deleted"]);
        }
    }
}

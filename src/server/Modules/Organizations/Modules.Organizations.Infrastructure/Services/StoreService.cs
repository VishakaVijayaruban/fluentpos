// --------------------------------------------------------------------------------------------------
// <copyright file="StoreService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Modules.Organizations.Core.Exceptions;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly IOrganizationDbContext _context;

        public StoreService(IOrganizationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid storeId)
        {
            return await _context.Stores.AsNoTracking().AnyAsync(s => s.Id == storeId);
        }

        public async Task<Guid> GetDefaultStoreIdAsync()
        {
            var defaultStoreId = await _context.Stores.AsNoTracking()
                .Where(s => s.IsDefault)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (defaultStoreId == Guid.Empty)
            {
                defaultStoreId = await _context.Stores.AsNoTracking()
                    .OrderBy(s => s.Id)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
            }

            if (defaultStoreId == Guid.Empty)
            {
                throw new OrganizationException("No stores are configured.", HttpStatusCode.InternalServerError);
            }

            return defaultStoreId;
        }
    }
}

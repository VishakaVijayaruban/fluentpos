// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Organizations.Core.Abstractions;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Organizations.Infrastructure.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationDbContext _context;

        public OrganizationService(IOrganizationDbContext context)
        {
            _context = context;
        }

        public async Task<StoreOrganizationInfo> GetStoreOrganizationAsync(Guid storeId)
        {
            return await _context.Stores.AsNoTracking()
                .Where(s => s.Id == storeId)
                .Select(s => new StoreOrganizationInfo(s.OrganizationId, s.Organization.Name, s.Organization.RoyaltyRatePercent))
                .FirstOrDefaultAsync();
        }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="CustomerService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.People.Core.Abstractions;
using FluentPOS.Shared.Core.IntegrationServices.People;
using FluentPOS.Shared.DTOs.People.Customers;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.People.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IPeopleDbContext _context;

        public CustomerService(IPeopleDbContext context)
        {
            _context = context;
        }

        public async Task<GetCustomerByIdResponse> GetDetailsAsync(Guid customerId)
        {
            return await _context.Customers.AsNoTracking()
                .Where(c => c.Id == customerId)
                .Select(c => new GetCustomerByIdResponse(c.Id, c.Name, c.Phone, c.Email, c.ImageUrl, c.Type))
                .FirstOrDefaultAsync();
        }
    }
}

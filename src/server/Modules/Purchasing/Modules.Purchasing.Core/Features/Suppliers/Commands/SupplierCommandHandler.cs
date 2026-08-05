// --------------------------------------------------------------------------------------------------
// <copyright file="SupplierCommandHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Modules.Purchasing.Core.Enums;
using FluentPOS.Modules.Purchasing.Core.Exceptions;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands
{
    internal class SupplierCommandHandler :
        IRequestHandler<RegisterSupplierCommand, Result<Guid>>,
        IRequestHandler<UpdateSupplierCommand, Result<Guid>>,
        IRequestHandler<RemoveSupplierCommand, Result<Guid>>
    {
        private readonly IPurchasingDbContext _context;
        private readonly IStringLocalizer<SupplierCommandHandler> _localizer;

        public SupplierCommandHandler(
            IPurchasingDbContext context,
            IStringLocalizer<SupplierCommandHandler> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RegisterSupplierCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var supplier = new Supplier
            {
                Name = command.Name,
                ContactName = command.ContactName,
                Email = command.Email,
                Phone = command.Phone,
                AddressLine = command.AddressLine,
                City = command.City,
                Postcode = command.Postcode
            };

            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(supplier.Id, _localizer["Supplier Saved"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (supplier == null)
            {
                throw new PurchasingException(_localizer["Supplier Not Found!"], HttpStatusCode.NotFound);
            }

            supplier.Name = command.Name;
            supplier.ContactName = command.ContactName;
            supplier.Email = command.Email;
            supplier.Phone = command.Phone;
            supplier.AddressLine = command.AddressLine;
            supplier.City = command.City;
            supplier.Postcode = command.Postcode;
            supplier.IsActive = command.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(supplier.Id, _localizer["Supplier Updated"]);
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<Guid>> Handle(RemoveSupplierCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
            if (supplier == null)
            {
                throw new PurchasingException(_localizer["Supplier Not Found!"], HttpStatusCode.NotFound);
            }

            bool hasOpenOrders = await _context.PurchaseOrders.AnyAsync(
                po => po.SupplierId == command.Id && (po.Status == PurchaseOrderStatus.Draft || po.Status == PurchaseOrderStatus.Submitted),
                cancellationToken);
            if (hasOpenOrders)
            {
                throw new PurchasingException(_localizer["Supplier has open purchase orders and cannot be removed."], HttpStatusCode.BadRequest);
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(supplier.Id, _localizer["Supplier Deleted"]);
        }
    }
}

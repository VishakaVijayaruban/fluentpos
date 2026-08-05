// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterSupplierCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands
{
    public class RegisterSupplierCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }

        public string ContactName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }
    }
}

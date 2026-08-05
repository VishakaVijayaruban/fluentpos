// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterStoreCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Stores.Commands
{
    public class RegisterStoreCommand : IRequest<Result<Guid>>
    {
        public Guid? OrganizationId { get; set; }

        public string Name { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public string Phone { get; set; }
    }
}

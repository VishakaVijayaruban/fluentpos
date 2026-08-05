// --------------------------------------------------------------------------------------------------
// <copyright file="UpdateStoreCommand.cs" company="FluentPOS">
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
    public class UpdateStoreCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public string Phone { get; set; }

        // When set, moves the store to another organization (e.g. sale to a franchisee).
        public Guid? OrganizationId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

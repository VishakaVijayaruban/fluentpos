// --------------------------------------------------------------------------------------------------
// <copyright file="Store.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Organizations.Core.Entities
{
    public class Store : BaseEntity
    {
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        public string Name { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public string Phone { get; set; }

        // The store used when a request carries no store context (legacy clients, HQ users).
        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();
    }
}

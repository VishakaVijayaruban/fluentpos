// --------------------------------------------------------------------------------------------------
// <copyright file="Supplier.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Purchasing.Core.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }

        public string ContactName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="Terminal.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Organizations.Core.Entities
{
    public class Terminal : BaseEntity
    {
        public Guid StoreId { get; set; }

        public virtual Store Store { get; set; }

        public string Name { get; set; }

        // SHA-256 hash of the long-lived device key issued at registration (null = unregistered).
        public string DeviceKeyHash { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

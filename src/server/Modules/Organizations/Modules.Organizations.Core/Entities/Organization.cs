// --------------------------------------------------------------------------------------------------
// <copyright file="Organization.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Organizations.Core.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; }

        public string Detail { get; set; }

        public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}

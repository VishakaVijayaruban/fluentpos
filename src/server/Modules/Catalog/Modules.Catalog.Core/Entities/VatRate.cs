// --------------------------------------------------------------------------------------------------
// <copyright file="VatRate.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Catalog.Core.Entities
{
    public class VatRate : BaseEntity
    {
        public string Name { get; set; }

        // Percentage, e.g. 20 for UK standard rate.
        public decimal Rate { get; set; }
    }
}

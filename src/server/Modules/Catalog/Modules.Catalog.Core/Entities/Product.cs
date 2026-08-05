// --------------------------------------------------------------------------------------------------
// <copyright file="Product.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Modules.Catalog.Core.Entities.ExtendedAttributes;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Catalog.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }

        public string LocaleName { get; set; }

        public Guid BrandId { get; set; }

        public virtual Brand Brand { get; set; }

        public Guid CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public decimal Price { get; set; }

        public decimal Cost { get; set; }

        public string ImageUrl { get; set; }

        // The VAT rate table is the single source of truth for tax.
        public Guid VatRateId { get; set; }

        public virtual VatRate VatRate { get; set; }

        // The scanned value (EAN/UPC/PLU); BarcodeSymbology only names the encoding.
        public string Barcode { get; set; }

        public string BarcodeSymbology { get; set; }

        // Challenge 25: selling this product requires an age check at the till.
        public bool IsAgeRestricted { get; set; }

        public int MinimumAge { get; set; } = 18;

        public string Detail { get; set; }

        public virtual ICollection<ProductExtendedAttribute> ExtendedAttributes { get; set; }

        public Product()
            : base()
        {
            ExtendedAttributes = new HashSet<ProductExtendedAttribute>();
        }
    }
}
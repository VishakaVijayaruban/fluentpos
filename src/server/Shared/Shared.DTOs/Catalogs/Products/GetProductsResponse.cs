// --------------------------------------------------------------------------------------------------
// <copyright file="GetProductsResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Products
{
    // Tax is the applied VAT percentage, resolved from the linked VAT rate.
    public record GetProductsResponse(Guid Id, string Name, string LocaleName, string Barcode, string BarcodeSymbology, string Detail, Guid BrandId, string BrandName, Guid CategoryId, string CategoryName, decimal Price, decimal Cost, string ImageUrl, decimal Tax, Guid VatRateId);
}
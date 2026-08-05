// --------------------------------------------------------------------------------------------------
// <copyright file="GetSuppliersResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Purchasing.Suppliers
{
    public record GetSuppliersResponse(Guid Id, string Name, string ContactName, string Email, string Phone, string AddressLine, string City, string Postcode, bool IsActive);
}

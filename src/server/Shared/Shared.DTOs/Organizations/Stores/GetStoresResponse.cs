// --------------------------------------------------------------------------------------------------
// <copyright file="GetStoresResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Organizations.Stores
{
    public record GetStoresResponse(Guid Id, Guid OrganizationId, string Name, string AddressLine, string City, string Postcode, string Phone, bool IsDefault, bool IsActive);
}

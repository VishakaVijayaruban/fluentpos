// --------------------------------------------------------------------------------------------------
// <copyright file="GetTerminalsResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Organizations.Terminals
{
    public record GetTerminalsResponse(Guid Id, Guid StoreId, string Name, bool IsActive);
}

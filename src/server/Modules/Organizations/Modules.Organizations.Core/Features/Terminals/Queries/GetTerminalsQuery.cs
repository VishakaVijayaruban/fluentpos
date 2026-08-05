// --------------------------------------------------------------------------------------------------
// <copyright file="GetTerminalsQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Organizations.Terminals;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Terminals.Queries
{
    public class GetTerminalsQuery : IRequest<Result<List<GetTerminalsResponse>>>
    {
        public Guid? StoreId { get; set; }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="GetTillSessionsQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Sales.Tills;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Queries
{
    public class GetTillSessionsQuery : IRequest<Result<List<GetTillSessionsResponse>>>
    {
        public Guid? StoreId { get; set; }

        public string Status { get; set; }
    }
}

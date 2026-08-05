// --------------------------------------------------------------------------------------------------
// <copyright file="GetTillSessionByIdQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Sales.Tills;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Queries
{
    /// <summary>
    /// Session detail including live X-report figures (cash takings, movements, running expected cash).
    /// </summary>
    public class GetTillSessionByIdQuery : IRequest<Result<GetTillSessionsResponse>>
    {
        public Guid Id { get; set; }
    }
}

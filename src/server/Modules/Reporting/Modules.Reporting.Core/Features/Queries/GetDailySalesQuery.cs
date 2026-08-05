// --------------------------------------------------------------------------------------------------
// <copyright file="GetDailySalesQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Reporting;
using MediatR;

namespace FluentPOS.Modules.Reporting.Core.Features.Queries
{
    public class GetDailySalesQuery : IRequest<Result<List<GetDailySalesResponse>>>
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public Guid? StoreId { get; set; }
    }

    public class GetRoyaltiesQuery : IRequest<Result<List<GetRoyaltySummaryResponse>>>
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}

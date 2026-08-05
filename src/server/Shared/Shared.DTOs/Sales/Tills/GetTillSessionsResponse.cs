// --------------------------------------------------------------------------------------------------
// <copyright file="GetTillSessionsResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Sales.Tills
{
    public record GetTillSessionsResponse(Guid Id, Guid StoreId, Guid TerminalId, string Status, Guid OpenedByUserId, DateTime OpenedAt, decimal OpeningFloat, Guid? ClosedByUserId, DateTime? ClosedAt, decimal? CountedCash, decimal? ExpectedCash, decimal? Variance, string Notes)
    {
        // X-report figures (live for open sessions, final for closed ones).
        public decimal CashPaymentsTotal { get; set; }

        public decimal PayInsTotal { get; set; }

        public decimal PayOutsTotal { get; set; }

        public decimal RunningExpectedCash { get; set; }
    }
}

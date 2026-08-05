// --------------------------------------------------------------------------------------------------
// <copyright file="GetDailySalesResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Reporting
{
    public record GetDailySalesResponse(Guid StoreId, Guid OrganizationId, string OrganizationName, DateTime Date, int OrdersCount, decimal GrossSales, decimal TaxTotal, decimal RefundsTotal, decimal NetSales, decimal RoyaltyRatePercent, decimal RoyaltyAmount);

    public record GetRoyaltySummaryResponse(Guid OrganizationId, string OrganizationName, decimal RoyaltyRatePercent, decimal NetSales, decimal RoyaltyAmount);
}

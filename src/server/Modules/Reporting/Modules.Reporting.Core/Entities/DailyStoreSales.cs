// --------------------------------------------------------------------------------------------------
// <copyright file="DailyStoreSales.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Reporting.Core.Entities
{
    /// <summary>
    /// Read model: one row per store per day, projected from sales integration events.
    /// Organization data is snapshotted at projection time so franchise reporting and
    /// royalty accrual never need cross-module joins.
    /// </summary>
    public class DailyStoreSales : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public Guid OrganizationId { get; private set; }

        public string OrganizationName { get; private set; }

        public decimal RoyaltyRatePercent { get; private set; }

        public DateTime Date { get; private set; }

        public int OrdersCount { get; private set; }

        public decimal GrossSales { get; private set; }

        public decimal TaxTotal { get; private set; }

        public decimal RefundsTotal { get; private set; }

        public decimal NetSales { get; private set; }

        public decimal RoyaltyAmount { get; private set; }

        public static DailyStoreSales Start(Guid storeId, DateTime date, Guid organizationId, string organizationName, decimal royaltyRatePercent)
        {
            return new DailyStoreSales
            {
                StoreId = storeId,
                Date = date.Date,
                OrganizationId = organizationId,
                OrganizationName = organizationName,
                RoyaltyRatePercent = royaltyRatePercent
            };
        }

        public void ApplySale(decimal total, decimal tax)
        {
            OrdersCount++;
            GrossSales += total;
            TaxTotal += tax;
            Recalculate();
        }

        public void ApplyRefund(decimal refundedTotal)
        {
            RefundsTotal += refundedTotal;
            Recalculate();
        }

        private void Recalculate()
        {
            NetSales = GrossSales - RefundsTotal;
            RoyaltyAmount = Math.Round(NetSales * RoyaltyRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        }
    }
}

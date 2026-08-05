using System;
using FluentPOS.Modules.Reporting.Core.Entities;
using Xunit;

namespace FluentPOS.Modules.Reporting.Core.Tests.Entities.Tests
{
    public class DailyStoreSalesShould
    {
        private static DailyStoreSales StartRow(decimal royaltyRate = 5m)
            => DailyStoreSales.Start(Guid.NewGuid(), new DateTime(2026, 8, 5), Guid.NewGuid(), "Northern Franchise Ltd", royaltyRate);

        [Fact]
        public void Accumulates_sales_and_computes_royalty()
        {
            // Arrange
            var row = StartRow(royaltyRate: 5m);

            // Act
            row.ApplySale(total: 120m, tax: 20m);
            row.ApplySale(total: 240m, tax: 40m);

            // Assert
            Assert.Equal(2, row.OrdersCount);
            Assert.Equal(360m, row.GrossSales);
            Assert.Equal(60m, row.TaxTotal);
            Assert.Equal(360m, row.NetSales);
            Assert.Equal(18m, row.RoyaltyAmount); // 5% of 360
        }

        [Fact]
        public void Reduces_net_and_royalty_when_refund_is_applied()
        {
            // Arrange
            var row = StartRow(royaltyRate: 5m);
            row.ApplySale(total: 360m, tax: 60m);

            // Act
            row.ApplyRefund(refundedTotal: 120m);

            // Assert
            Assert.Equal(360m, row.GrossSales);
            Assert.Equal(120m, row.RefundsTotal);
            Assert.Equal(240m, row.NetSales);
            Assert.Equal(12m, row.RoyaltyAmount); // 5% of 240
        }

        [Fact]
        public void Accrues_no_royalty_for_the_franchisor_own_stores()
        {
            // Arrange
            var row = StartRow(royaltyRate: 0m);

            // Act
            row.ApplySale(total: 500m, tax: 80m);

            // Assert
            Assert.Equal(500m, row.NetSales);
            Assert.Equal(0m, row.RoyaltyAmount);
        }

        [Fact]
        public void Normalizes_date_to_midnight()
        {
            // Arrange & Act
            var row = DailyStoreSales.Start(Guid.NewGuid(), new DateTime(2026, 8, 5, 14, 33, 12), Guid.NewGuid(), "Org", 5m);

            // Assert
            Assert.Equal(new DateTime(2026, 8, 5), row.Date);
        }
    }
}

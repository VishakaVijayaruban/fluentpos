using System;
using System.Linq;
using FluentPOS.Modules.Sales.Core.Entities;
using Xunit;

namespace FluentPOS.Modules.Sales.Core.Tests.Entities.Tests
{
    public class OrderShould
    {
        [Fact]
        public void Creates_unpaid_order_with_utc_timestamp()
        {
            // Arrange & Act
            var order = Order.InitializeOrder();

            // Assert
            Assert.False(order.IsPaid);
            Assert.Equal(DateTimeKind.Utc, order.TimeStamp.Kind);
        }

        [Fact]
        public void Computes_line_totals_from_vat_percentage_when_product_is_added()
        {
            // Arrange
            var order = Order.InitializeOrder();
            var productId = Guid.NewGuid();

            // Act
            order.AddProduct(productId, "London Dry Gin", quantity: 2, rate: 10m, vatRatePercent: 20m);

            // Assert
            var line = Assert.Single(order.Products);
            Assert.Equal(productId, line.ProductId);
            Assert.Equal(2, line.Quantity);
            Assert.Equal(20m, line.Price);
            Assert.Equal(4m, line.Tax);
            Assert.Equal(24m, line.Total);
            Assert.Equal(20m, order.SubTotal);
            Assert.Equal(4m, order.Tax);
            Assert.Equal(24m, order.Total);
        }

        [Fact]
        public void Accumulates_lines_when_multiple_products_are_added()
        {
            // Arrange
            var order = Order.InitializeOrder();

            // Act
            order.AddProduct(Guid.NewGuid(), "Gin", quantity: 1, rate: 10m, vatRatePercent: 20m);
            order.AddProduct(Guid.NewGuid(), "Tonic", quantity: 4, rate: 1.5m, vatRatePercent: 0m);

            // Assert
            Assert.Equal(2, order.Products.Count);
            Assert.Equal(12m, order.Products.First().Total);
            Assert.Equal(6m, order.Products.Last().Total);
            Assert.Equal(16m, order.SubTotal);
            Assert.Equal(2m, order.Tax);
            Assert.Equal(18m, order.Total);
        }

        [Fact]
        public void Marks_order_as_paid()
        {
            // Arrange
            var order = Order.InitializeOrder();

            // Act
            order.MarkAsPaid();

            // Assert
            Assert.True(order.IsPaid);
        }

        [Fact]
        public void Sets_reference_number()
        {
            // Arrange
            var order = Order.InitializeOrder();

            // Act
            order.SetReferenceNumber("ORD-0001");

            // Assert
            Assert.Equal("ORD-0001", order.ReferenceNumber);
        }
    }
}

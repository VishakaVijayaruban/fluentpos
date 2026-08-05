using System;
using System.Linq;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Modules.Purchasing.Core.Enums;
using Xunit;

namespace FluentPOS.Modules.Purchasing.Core.Tests.Entities.Tests
{
    public class PurchaseOrderShould
    {
        [Fact]
        public void Accumulates_total_when_items_are_added()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid() };

            // Act
            order.AddItem(Guid.NewGuid(), "Gin", quantity: 6, unitCost: 12m);
            order.AddItem(Guid.NewGuid(), "Tonic", quantity: 24, unitCost: 0.5m);

            // Assert
            Assert.Equal(2, order.Items.Count);
            Assert.Equal(84m, order.Total);
        }

        [Fact]
        public void Merges_quantity_when_same_product_is_added_again()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid() };
            var productId = Guid.NewGuid();

            // Act
            order.AddItem(productId, "Gin", quantity: 6, unitCost: 12m);
            order.AddItem(productId, "Gin", quantity: 6, unitCost: 12m);

            // Assert
            var line = Assert.Single(order.Items);
            Assert.Equal(12m, line.Quantity);
            Assert.Equal(144m, order.Total);
        }

        [Fact]
        public void Throws_when_submitted_without_supplier()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid() };
            order.AddItem(Guid.NewGuid(), "Gin", 6, 12m);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => order.Submit());
        }

        [Fact]
        public void Transitions_to_received_only_from_submitted()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid(), SupplierId = Guid.NewGuid() };
            order.AddItem(Guid.NewGuid(), "Gin", 6, 12m);

            // Act & Assert: receiving a draft is invalid
            Assert.Throws<InvalidOperationException>(() => order.MarkAsReceived());

            // Act: submit then receive
            order.Submit();
            order.MarkAsReceived();

            // Assert
            Assert.Equal(PurchaseOrderStatus.Received, order.Status);
        }

        [Fact]
        public void Cannot_cancel_a_received_order()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid(), SupplierId = Guid.NewGuid() };
            order.AddItem(Guid.NewGuid(), "Gin", 6, 12m);
            order.Submit();
            order.MarkAsReceived();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => order.Cancel());
        }

        [Fact]
        public void Cancels_a_draft_order()
        {
            // Arrange
            var order = new PurchaseOrder { StoreId = Guid.NewGuid() };
            order.AddItem(Guid.NewGuid(), "Gin", 6, 12m);

            // Act
            order.Cancel();

            // Assert
            Assert.Equal(PurchaseOrderStatus.Cancelled, order.Status);
        }
    }
}

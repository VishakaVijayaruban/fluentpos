using System;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using Xunit;

namespace FluentPOS.Modules.Sales.Core.Tests.Entities.Tests
{
    public class TransactionShould
    {
        [Fact]
        public void Records_payment_details_for_an_order()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var storeId = Guid.NewGuid();

            // Act
            var transaction = Transaction.Record(orderId, PaymentType.Cash, amount: 22m, tenderedAmount: 25m, note: "Paid in cash", storeId: storeId);

            // Assert
            Assert.Equal(orderId, transaction.OrderId);
            Assert.Equal(storeId, transaction.StoreId);
            Assert.Equal(PaymentType.Cash, transaction.PaymentType);
            Assert.Equal(22m, transaction.Amount);
            Assert.Equal(25m, transaction.TenderedAmount);
            Assert.Equal("Paid in cash", transaction.Note);
        }

        [Fact]
        public void Records_timestamp_in_utc()
        {
            // Arrange & Act
            var transaction = Transaction.Record(Guid.NewGuid(), PaymentType.CreditCard, amount: 10m, tenderedAmount: 10m, note: null, storeId: Guid.NewGuid());

            // Assert
            Assert.Equal(DateTimeKind.Utc, transaction.TimeStamp.Kind);
        }
    }
}

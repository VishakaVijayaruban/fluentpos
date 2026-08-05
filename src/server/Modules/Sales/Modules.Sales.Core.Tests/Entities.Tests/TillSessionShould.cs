using System;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Modules.Sales.Core.Enums;
using Xunit;

namespace FluentPOS.Modules.Sales.Core.Tests.Entities.Tests
{
    public class TillSessionShould
    {
        [Fact]
        public void Opens_with_float_and_open_status()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var terminalId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var session = TillSession.Open(storeId, terminalId, userId, openingFloat: 50m);

            // Assert
            Assert.Equal(TillSessionStatus.Open, session.Status);
            Assert.Equal(storeId, session.StoreId);
            Assert.Equal(terminalId, session.TerminalId);
            Assert.Equal(userId, session.OpenedByUserId);
            Assert.Equal(50m, session.OpeningFloat);
            Assert.Equal(DateTimeKind.Utc, session.OpenedAt.Kind);
        }

        [Fact]
        public void Computes_variance_when_closed()
        {
            // Arrange
            var session = TillSession.Open(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), openingFloat: 50m);
            var closer = Guid.NewGuid();

            // Act
            session.Close(countedCash: 25m, expectedCash: 30m, closedByUserId: closer, notes: "End of day");

            // Assert
            Assert.Equal(TillSessionStatus.Closed, session.Status);
            Assert.Equal(25m, session.CountedCash);
            Assert.Equal(30m, session.ExpectedCash);
            Assert.Equal(-5m, session.Variance);
            Assert.Equal(closer, session.ClosedByUserId);
            Assert.NotNull(session.ClosedAt);
        }

        [Fact]
        public void Throws_when_closed_twice()
        {
            // Arrange
            var session = TillSession.Open(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), openingFloat: 50m);
            session.Close(30m, 30m, Guid.NewGuid(), null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => session.Close(30m, 30m, Guid.NewGuid(), null));
        }
    }
}

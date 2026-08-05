// --------------------------------------------------------------------------------------------------
// <copyright file="TillSession.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Sales.Core.Entities
{
    public class TillSession : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public Guid TerminalId { get; private set; }

        public TillSessionStatus Status { get; private set; } = TillSessionStatus.Open;

        public Guid OpenedByUserId { get; private set; }

        public DateTime OpenedAt { get; private set; }

        public decimal OpeningFloat { get; private set; }

        public Guid? ClosedByUserId { get; private set; }

        public DateTime? ClosedAt { get; private set; }

        // Cash physically counted in the drawer at close.
        public decimal? CountedCash { get; private set; }

        // Float + cash payments +/- cash movements, snapshotted at close (the Z report).
        public decimal? ExpectedCash { get; private set; }

        public decimal? Variance { get; private set; }

        public string Notes { get; private set; }

        public static TillSession Open(Guid storeId, Guid terminalId, Guid openedByUserId, decimal openingFloat)
        {
            return new TillSession
            {
                StoreId = storeId,
                TerminalId = terminalId,
                OpenedByUserId = openedByUserId,
                OpenedAt = DateTime.UtcNow,
                OpeningFloat = openingFloat
            };
        }

        public void Close(decimal countedCash, decimal expectedCash, Guid closedByUserId, string notes)
        {
            if (Status == TillSessionStatus.Closed)
            {
                throw new InvalidOperationException("Till session is already closed.");
            }

            Status = TillSessionStatus.Closed;
            ClosedByUserId = closedByUserId;
            ClosedAt = DateTime.UtcNow;
            CountedCash = countedCash;
            ExpectedCash = expectedCash;
            Variance = countedCash - expectedCash;
            Notes = notes;
        }
    }
}

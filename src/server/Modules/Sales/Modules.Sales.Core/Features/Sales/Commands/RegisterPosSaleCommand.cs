// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterPosSaleCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands
{
    /// <summary>
    /// Offline-capable checkout: the POS client owns the basket and submits the complete
    /// sale document. ClientSaleId is generated on the device and doubles as the order id,
    /// so a queued sale can be replayed safely after connectivity loss — the second attempt
    /// returns the already-created order instead of double-charging.
    /// </summary>
    public class RegisterPosSaleCommand : IRequest<Result<Guid>>
    {
        public Guid ClientSaleId { get; set; }

        // Optional: store-scoped tokens carry their store; head office may specify.
        public Guid? StoreId { get; set; }

        // Defaults to the walk-in customer for anonymous counter sales.
        public Guid? CustomerId { get; set; }

        public Guid? TillSessionId { get; set; }

        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        public decimal TenderedAmount { get; set; }

        public bool AgeVerified { get; set; }

        // When the sale actually happened on the device (informational; server time remains authoritative).
        public DateTime? OccurredAt { get; set; }

        public string Note { get; set; }

        public List<PosSaleItem> Items { get; set; } = new();
    }

    public class PosSaleItem
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}

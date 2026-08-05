// --------------------------------------------------------------------------------------------------
// <copyright file="OrderRegisteredEvent.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Shared.Core.IntegrationEvents.Sales
{
    /// <summary>
    /// Raised when a sale completes. Lives in Shared.Core so other modules (reporting,
    /// royalties, future webhooks) can subscribe without referencing the Sales module.
    /// </summary>
    public class OrderRegisteredEvent : Event
    {
        public Guid OrderId { get; }

        public Guid StoreId { get; }

        public string ReferenceNumber { get; }

        public decimal SubTotal { get; }

        public decimal Tax { get; }

        public decimal Total { get; }

        public DateTime OccurredOn { get; }

        public OrderRegisteredEvent(Guid orderId, Guid storeId, string referenceNumber, decimal subTotal, decimal tax, decimal total, DateTime occurredOn)
        {
            OrderId = orderId;
            StoreId = storeId;
            ReferenceNumber = referenceNumber;
            SubTotal = subTotal;
            Tax = tax;
            Total = total;
            OccurredOn = occurredOn;
            AggregateId = orderId;
            RelatedEntities = Array.Empty<Type>();
        }
    }
}

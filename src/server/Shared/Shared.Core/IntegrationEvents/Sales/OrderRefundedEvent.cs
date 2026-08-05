// --------------------------------------------------------------------------------------------------
// <copyright file="OrderRefundedEvent.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Shared.Core.IntegrationEvents.Sales
{
    public class OrderRefundedEvent : Event
    {
        public Guid OrderId { get; }

        public Guid StoreId { get; }

        public string ReferenceNumber { get; }

        public decimal RefundedTotal { get; }

        public DateTime OccurredOn { get; }

        public OrderRefundedEvent(Guid orderId, Guid storeId, string referenceNumber, decimal refundedTotal, DateTime occurredOn)
        {
            OrderId = orderId;
            StoreId = storeId;
            ReferenceNumber = referenceNumber;
            RefundedTotal = refundedTotal;
            OccurredOn = occurredOn;
            AggregateId = orderId;
            RelatedEntities = Array.Empty<Type>();
        }
    }
}

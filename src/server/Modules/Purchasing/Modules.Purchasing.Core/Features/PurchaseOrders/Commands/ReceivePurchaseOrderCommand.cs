// --------------------------------------------------------------------------------------------------
// <copyright file="ReceivePurchaseOrderCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands
{
    public class ReceivePurchaseOrderCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        // Per-line received quantities; lines omitted are received in full.
        public List<ReceivePurchaseOrderItem> Items { get; set; } = new();
    }

    public class ReceivePurchaseOrderItem
    {
        public Guid ProductId { get; set; }

        public decimal ReceivedQuantity { get; set; }
    }
}

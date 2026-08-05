// --------------------------------------------------------------------------------------------------
// <copyright file="SubmitPurchaseOrderCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands
{
    public class SubmitPurchaseOrderCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        // Allows assigning/overriding the supplier at submit time (drafts may have none).
        public Guid? SupplierId { get; set; }
    }
}

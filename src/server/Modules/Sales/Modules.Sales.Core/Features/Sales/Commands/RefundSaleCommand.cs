// --------------------------------------------------------------------------------------------------
// <copyright file="RefundSaleCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands
{
    /// <summary>
    /// Full-order refund: reverses the payment and returns the goods to stock.
    /// </summary>
    public class RefundSaleCommand : IRequest<Result<Guid>>
    {
        public Guid OrderId { get; set; }

        public string Reason { get; set; }

        // Till session the refund is paid out from, when done at a till.
        public Guid? TillSessionId { get; set; }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterSaleCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands
{
    public class RegisterSaleCommand : IRequest<Result<Guid>>
    {
        public Guid CartId { get; set; }

        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        public decimal TenderedAmount { get; set; }

        public string Note { get; set; }
    }
}
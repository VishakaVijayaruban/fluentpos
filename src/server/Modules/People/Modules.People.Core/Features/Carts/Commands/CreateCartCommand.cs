// --------------------------------------------------------------------------------------------------
// <copyright file="CreateCartCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.People.Core.Features.Carts.Commands
{
    public class CreateCartCommand : IRequest<Result<Guid>>
    {
        public Guid CustomerId { get; set; }

        // Optional: store-scoped users inherit their token's store; head-office users
        // fall back to the default store when omitted.
        public Guid? StoreId { get; set; }
    }
}
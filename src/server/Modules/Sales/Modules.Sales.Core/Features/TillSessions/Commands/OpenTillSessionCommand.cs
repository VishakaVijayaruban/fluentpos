// --------------------------------------------------------------------------------------------------
// <copyright file="OpenTillSessionCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Commands
{
    public class OpenTillSessionCommand : IRequest<Result<Guid>>
    {
        public Guid TerminalId { get; set; }

        public decimal OpeningFloat { get; set; }

        // Optional for store-scoped users; required context for head office comes from this.
        public Guid? StoreId { get; set; }
    }
}

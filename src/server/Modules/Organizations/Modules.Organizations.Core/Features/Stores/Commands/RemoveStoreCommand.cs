// --------------------------------------------------------------------------------------------------
// <copyright file="RemoveStoreCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Stores.Commands
{
    public class RemoveStoreCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
    }
}

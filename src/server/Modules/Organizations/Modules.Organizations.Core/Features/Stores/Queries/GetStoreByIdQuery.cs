// --------------------------------------------------------------------------------------------------
// <copyright file="GetStoreByIdQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Organizations.Stores;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Stores.Queries
{
    public class GetStoreByIdQuery : IRequest<Result<GetStoresResponse>>
    {
        public Guid Id { get; set; }
    }
}

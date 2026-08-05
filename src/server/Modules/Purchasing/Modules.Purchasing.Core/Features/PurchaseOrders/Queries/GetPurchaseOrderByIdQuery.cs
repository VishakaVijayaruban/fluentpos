// --------------------------------------------------------------------------------------------------
// <copyright file="GetPurchaseOrderByIdQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Purchasing.PurchaseOrders;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Queries
{
    public class GetPurchaseOrderByIdQuery : IRequest<Result<GetPurchaseOrdersResponse>>
    {
        public Guid Id { get; set; }
    }
}

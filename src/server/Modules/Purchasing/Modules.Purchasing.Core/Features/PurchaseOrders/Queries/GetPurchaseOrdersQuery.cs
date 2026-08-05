// --------------------------------------------------------------------------------------------------
// <copyright file="GetPurchaseOrdersQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Purchasing.PurchaseOrders;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Queries
{
    public class GetPurchaseOrdersQuery : IRequest<Result<List<GetPurchaseOrdersResponse>>>
    {
        public Guid? StoreId { get; set; }

        public string Status { get; set; }
    }
}

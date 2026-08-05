// --------------------------------------------------------------------------------------------------
// <copyright file="GetPurchaseOrdersResponse.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace FluentPOS.Shared.DTOs.Purchasing.PurchaseOrders
{
    public record GetPurchaseOrdersResponse(Guid Id, Guid StoreId, Guid? SupplierId, string SupplierName, string ReferenceNumber, string Status, DateTime TimeStamp, string Notes, decimal Total)
    {
        public ICollection<PurchaseOrderItemResponse> Items { get; set; }
    }

    public record PurchaseOrderItemResponse(Guid Id, Guid ProductId, string ProductName, decimal Quantity, decimal UnitCost, decimal ReceivedQuantity);
}

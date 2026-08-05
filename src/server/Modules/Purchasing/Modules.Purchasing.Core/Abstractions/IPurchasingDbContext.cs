// --------------------------------------------------------------------------------------------------
// <copyright file="IPurchasingDbContext.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Shared.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Purchasing.Core.Abstractions
{
    public interface IPurchasingDbContext : IDbContext
    {
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}

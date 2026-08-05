// --------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrder.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentPOS.Modules.Purchasing.Core.Enums;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Purchasing.Core.Entities
{
    public class PurchaseOrder : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        // Nullable while draft; must be assigned before the order can be submitted.
        public Guid? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }

        public string ReferenceNumber { get; private set; }

        public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;

        public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;

        public string Notes { get; set; }

        public decimal Total { get; private set; }

        public virtual ICollection<PurchaseOrderItem> Items { get; private set; } = new List<PurchaseOrderItem>();

        public void SetReferenceNumber(string referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public void AddItem(Guid productId, string productName, decimal quantity, decimal unitCost)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                Items.Add(new PurchaseOrderItem
                {
                    PurchaseOrderId = Id,
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitCost = unitCost
                });
            }

            RecalculateTotal();
        }

        public void Submit()
        {
            EnsureStatus(PurchaseOrderStatus.Draft);
            if (SupplierId == null)
            {
                throw new InvalidOperationException("A supplier must be assigned before submitting a purchase order.");
            }

            Status = PurchaseOrderStatus.Submitted;
        }

        public void MarkAsReceived()
        {
            EnsureStatus(PurchaseOrderStatus.Submitted);
            Status = PurchaseOrderStatus.Received;
        }

        public void Cancel()
        {
            if (Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
            {
                throw new InvalidOperationException($"A {Status} purchase order cannot be cancelled.");
            }

            Status = PurchaseOrderStatus.Cancelled;
        }

        private void EnsureStatus(PurchaseOrderStatus expected)
        {
            if (Status != expected)
            {
                throw new InvalidOperationException($"Purchase order must be {expected} for this operation (current: {Status}).");
            }
        }

        private void RecalculateTotal()
        {
            Total = Items.Sum(i => i.Quantity * i.UnitCost);
        }
    }
}

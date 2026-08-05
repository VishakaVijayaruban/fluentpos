// --------------------------------------------------------------------------------------------------
// <copyright file="Transaction.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Modules.Sales.Core.Enums;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Sales.Core.Entities
{
    public class Transaction : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public Guid OrderId { get; set; }

        public Guid? TillSessionId { get; set; }

        public PaymentType PaymentType { get; set; }

        public DateTime TimeStamp { get; private set; }

        public decimal Amount { get; set; }

        public decimal TenderedAmount { get; set; }

        public string Note { get; set; }

        public static Transaction Record(Guid orderId, PaymentType paymentType, decimal amount, decimal tenderedAmount, string note, Guid storeId, Guid? tillSessionId = null)
        {
            return new Transaction
            {
                OrderId = orderId,
                StoreId = storeId,
                TillSessionId = tillSessionId,
                PaymentType = paymentType,
                Amount = amount,
                TenderedAmount = tenderedAmount,
                Note = note,
                TimeStamp = DateTime.UtcNow
            };
        }
    }
}
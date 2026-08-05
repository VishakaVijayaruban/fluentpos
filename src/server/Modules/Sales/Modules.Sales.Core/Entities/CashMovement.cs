// --------------------------------------------------------------------------------------------------
// <copyright file="CashMovement.cs" company="FluentPOS">
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
    public class CashMovement : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public Guid TillSessionId { get; private set; }

        public CashMovementKind Kind { get; private set; }

        public decimal Amount { get; private set; }

        public string Reason { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public static CashMovement Record(Guid storeId, Guid tillSessionId, CashMovementKind kind, decimal amount, string reason)
        {
            return new CashMovement
            {
                StoreId = storeId,
                TillSessionId = tillSessionId,
                Kind = kind,
                Amount = amount,
                Reason = reason,
                TimeStamp = DateTime.UtcNow
            };
        }
    }
}

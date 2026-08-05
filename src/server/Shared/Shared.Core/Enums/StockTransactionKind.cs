// --------------------------------------------------------------------------------------------------
// <copyright file="StockTransactionKind.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace FluentPOS.Shared.Core.Enums
{
    public enum StockTransactionKind
    {
        // Decreases stock.
        Sale,

        // Increases stock (goods received).
        Purchase,

        // Increases stock (customer refund/return).
        Return
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="CashMovementKind.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace FluentPOS.Modules.Sales.Core.Enums
{
    public enum CashMovementKind
    {
        // Cash added to the drawer (e.g. change float top-up).
        PayIn,

        // Cash taken out of the drawer (e.g. safe drop, supplier payout).
        PayOut
    }
}

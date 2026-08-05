// --------------------------------------------------------------------------------------------------
// <copyright file="PosTokenRequest.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.DTOs.Identity.Tokens
{
    /// <summary>
    /// Operator sign-in at a registered till: the long-lived device key authenticates the
    /// terminal, the short PIN authenticates the cashier. The issued token is scoped to the
    /// terminal's store.
    /// </summary>
    public class PosTokenRequest
    {
        public Guid TerminalId { get; set; }

        public string DeviceKey { get; set; }

        public string Email { get; set; }

        public string Pin { get; set; }
    }

    public class SetPosPinRequest
    {
        public string Pin { get; set; }
    }
}

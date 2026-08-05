// --------------------------------------------------------------------------------------------------
// <copyright file="ITerminalService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Core.IntegrationServices.Organization
{
    /// <summary>
    /// Integration Services for terminal device authentication.
    /// </summary>
    public interface ITerminalService
    {
        /// <summary>
        /// Validates a terminal's long-lived device key. Returns the terminal's store id
        /// when the key is valid and the terminal is active; null otherwise.
        /// </summary>
        Task<Guid?> ValidateDeviceKeyAsync(Guid terminalId, string deviceKey);
    }
}

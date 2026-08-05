// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterTerminalDeviceCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Terminals.Commands
{
    /// <summary>
    /// Issues a long-lived device key for a till. The plaintext key is returned exactly once;
    /// only its hash is stored. Re-running rotates the key.
    /// </summary>
    public class RegisterTerminalDeviceCommand : IRequest<Result<string>>
    {
        public Guid TerminalId { get; set; }
    }
}

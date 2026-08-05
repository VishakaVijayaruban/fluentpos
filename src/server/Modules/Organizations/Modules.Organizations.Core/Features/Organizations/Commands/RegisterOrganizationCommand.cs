// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterOrganizationCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Organizations.Core.Features.Organizations.Commands
{
    /// <summary>
    /// Onboards a franchisee organization with its royalty agreement.
    /// </summary>
    public class RegisterOrganizationCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }

        public string Detail { get; set; }

        public decimal RoyaltyRatePercent { get; set; }
    }
}

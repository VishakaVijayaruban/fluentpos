// --------------------------------------------------------------------------------------------------
// <copyright file="OpenTillSessionCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.TillSessions.Commands.Validators
{
    public class OpenTillSessionCommandValidator : AbstractValidator<OpenTillSessionCommand>
    {
        public OpenTillSessionCommandValidator(IStringLocalizer<OpenTillSessionCommandValidator> localizer)
        {
            RuleFor(c => c.TerminalId)
                .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.OpeningFloat)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."]);
        }
    }
}

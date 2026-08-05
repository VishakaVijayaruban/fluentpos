// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterTerminalCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Terminals.Commands.Validators
{
    public class RegisterTerminalCommandValidator : AbstractValidator<RegisterTerminalCommand>
    {
        public RegisterTerminalCommandValidator(IStringLocalizer<RegisterTerminalCommandValidator> localizer)
        {
            RuleFor(c => c.StoreId)
                .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage(localizer["The {PropertyName} property cannot be empty."])
                .Length(2, 100).WithMessage(localizer["The {PropertyName} property must have between 2 and 100 characters."]);
        }
    }
}

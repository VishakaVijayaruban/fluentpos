// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterOrganizationCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Organizations.Core.Features.Organizations.Commands.Validators
{
    public class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
    {
        public RegisterOrganizationCommandValidator(IStringLocalizer<RegisterOrganizationCommandValidator> localizer)
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage(localizer["The {PropertyName} property cannot be empty."])
                .Length(2, 150).WithMessage(localizer["The {PropertyName} property must have between 2 and 150 characters."]);
            RuleFor(c => c.RoyaltyRatePercent)
                .InclusiveBetween(0, 50).WithMessage(localizer["The {PropertyName} property must be between 0 and 50."]);
        }
    }
}

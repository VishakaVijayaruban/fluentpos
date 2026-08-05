// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterSupplierCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands.Validators
{
    public class RegisterSupplierCommandValidator : AbstractValidator<RegisterSupplierCommand>
    {
        public RegisterSupplierCommandValidator(IStringLocalizer<RegisterSupplierCommandValidator> localizer)
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage(localizer["The {PropertyName} property cannot be empty."])
                .Length(2, 150).WithMessage(localizer["The {PropertyName} property must have between 2 and 150 characters."]);
            RuleFor(c => c.Email)
                .EmailAddress().WithMessage(localizer["The {PropertyName} property must be a valid email address."])
                .When(c => !string.IsNullOrWhiteSpace(c.Email));
        }
    }
}

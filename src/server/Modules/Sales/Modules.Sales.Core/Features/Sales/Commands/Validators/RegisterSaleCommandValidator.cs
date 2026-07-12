// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterSaleCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands.Validators
{
    public class RegisterSaleCommandValidator : AbstractValidator<RegisterSaleCommand>
    {
        public RegisterSaleCommandValidator(IStringLocalizer<RegisterSaleCommandValidator> localizer)
        {
            RuleFor(c => c.CartId)
                .NotEmpty().WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.PaymentType)
                .IsInEnum().WithMessage(localizer["The {PropertyName} property has an invalid value."]);
            RuleFor(c => c.TenderedAmount)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."]);
        }
    }
}

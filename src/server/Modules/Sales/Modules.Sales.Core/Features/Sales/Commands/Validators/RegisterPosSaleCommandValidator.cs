// --------------------------------------------------------------------------------------------------
// <copyright file="RegisterPosSaleCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Commands.Validators
{
    public class RegisterPosSaleCommandValidator : AbstractValidator<RegisterPosSaleCommand>
    {
        public RegisterPosSaleCommandValidator(IStringLocalizer<RegisterPosSaleCommandValidator> localizer)
        {
            RuleFor(c => c.ClientSaleId)
                .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.PaymentType)
                .IsInEnum().WithMessage(localizer["The {PropertyName} property has an invalid value."]);
            RuleFor(c => c.TenderedAmount)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."]);
            RuleFor(c => c.Items)
                .NotEmpty().WithMessage(localizer["A sale must contain at least one item."]);
            RuleForEach(c => c.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage(localizer["The {PropertyName} property must be greater than zero."]);
            });
        }
    }
}

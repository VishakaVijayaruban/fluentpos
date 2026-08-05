// --------------------------------------------------------------------------------------------------
// <copyright file="RefundSaleCommandValidator.cs" company="FluentPOS">
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
    public class RefundSaleCommandValidator : AbstractValidator<RefundSaleCommand>
    {
        public RefundSaleCommandValidator(IStringLocalizer<RefundSaleCommandValidator> localizer)
        {
            // OrderId comes from the route after binding; not validated here.
            RuleFor(c => c.Reason)
                .NotEmpty().WithMessage(localizer["A refund reason is required."])
                .Length(3, 250).WithMessage(localizer["The {PropertyName} property must have between 3 and 250 characters."]);
        }
    }
}

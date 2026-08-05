// --------------------------------------------------------------------------------------------------
// <copyright file="RecordCashMovementCommandValidator.cs" company="FluentPOS">
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
    public class RecordCashMovementCommandValidator : AbstractValidator<RecordCashMovementCommand>
    {
        public RecordCashMovementCommandValidator(IStringLocalizer<RecordCashMovementCommandValidator> localizer)
        {
            // TillSessionId comes from the route after binding; not validated here.
            RuleFor(c => c.Kind)
                .IsInEnum().WithMessage(localizer["The {PropertyName} property has an invalid value."]);
            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage(localizer["The {PropertyName} property must be greater than zero."]);
            RuleFor(c => c.Reason)
                .NotEmpty().WithMessage(localizer["The {PropertyName} property cannot be empty."]);
        }
    }
}

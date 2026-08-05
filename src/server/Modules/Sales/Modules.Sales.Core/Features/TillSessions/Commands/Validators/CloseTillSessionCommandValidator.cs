// --------------------------------------------------------------------------------------------------
// <copyright file="CloseTillSessionCommandValidator.cs" company="FluentPOS">
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
    public class CloseTillSessionCommandValidator : AbstractValidator<CloseTillSessionCommand>
    {
        public CloseTillSessionCommandValidator(IStringLocalizer<CloseTillSessionCommandValidator> localizer)
        {
            // Id comes from the route after binding, so it must not be validated here
            // (auto-validation runs on the request body before the controller sets it).
            RuleFor(c => c.CountedCash)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."]);
        }
    }
}

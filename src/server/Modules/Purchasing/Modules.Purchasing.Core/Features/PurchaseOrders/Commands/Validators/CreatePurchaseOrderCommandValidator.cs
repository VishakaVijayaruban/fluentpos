// --------------------------------------------------------------------------------------------------
// <copyright file="CreatePurchaseOrderCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.PurchaseOrders.Commands.Validators
{
    public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
    {
        public CreatePurchaseOrderCommandValidator(IStringLocalizer<CreatePurchaseOrderCommandValidator> localizer)
        {
            RuleFor(c => c.Items)
                .NotEmpty().WithMessage(localizer["A purchase order must contain at least one item."]);
            RuleForEach(c => c.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage(localizer["The {PropertyName} property must be greater than zero."]);
                item.RuleFor(i => i.UnitCost)
                    .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."])
                    .When(i => i.UnitCost.HasValue);
            });
        }
    }
}

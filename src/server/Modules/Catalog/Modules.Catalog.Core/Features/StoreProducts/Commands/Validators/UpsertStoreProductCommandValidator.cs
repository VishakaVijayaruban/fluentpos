// --------------------------------------------------------------------------------------------------
// <copyright file="UpsertStoreProductCommandValidator.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Catalog.Core.Features.StoreProducts.Commands.Validators
{
    public class UpsertStoreProductCommandValidator : AbstractValidator<UpsertStoreProductCommand>
    {
        public UpsertStoreProductCommandValidator(IStringLocalizer<UpsertStoreProductCommandValidator> localizer)
        {
            RuleFor(c => c.StoreId)
                .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.ProductId)
                .NotEqual(Guid.Empty).WithMessage(localizer["The {PropertyName} property cannot be empty."]);
            RuleFor(c => c.Price)
                .GreaterThan(0).WithMessage(localizer["The {PropertyName} property must be greater than zero."])
                .When(c => c.Price.HasValue);
            RuleFor(c => c.ReorderPoint)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["The {PropertyName} property cannot be negative."])
                .When(c => c.ReorderPoint.HasValue);
            RuleFor(c => c.ReorderQuantity)
                .GreaterThan(0).WithMessage(localizer["The {PropertyName} property must be greater than zero."])
                .When(c => c.ReorderQuantity.HasValue);
        }
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="MvcBuilderExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Reflection;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Sales.Infrastructure.Extensions
{
    internal static class MvcBuilderExtensions
    {
        internal static IMvcBuilder AddSalesValidation(this IMvcBuilder builder)
        {
            builder.Services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(ISalesDbContext)));
            return builder;
        }
    }
}
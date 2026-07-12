// --------------------------------------------------------------------------------------------------
// <copyright file="GetVatRatesQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.VatRates;
using MediatR;

namespace FluentPOS.Modules.Catalog.Core.Features.VatRates.Queries
{
    public class GetVatRatesQuery : IRequest<Result<List<GetVatRatesResponse>>>
    {
    }
}

// --------------------------------------------------------------------------------------------------
// <copyright file="GetSuppliersQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Purchasing.Suppliers;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Queries
{
    public class GetSuppliersQuery : IRequest<Result<List<GetSuppliersResponse>>>
    {
    }
}

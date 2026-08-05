// --------------------------------------------------------------------------------------------------
// <copyright file="GetCatalogSyncQuery.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Sync;
using MediatR;

namespace FluentPOS.Modules.Catalog.Core.Features.Sync
{
    public class GetCatalogSyncQuery : IRequest<Result<CatalogSyncResponse>>
    {
        // Null means full pull.
        public DateTime? Since { get; set; }
    }
}

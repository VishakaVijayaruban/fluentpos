// --------------------------------------------------------------------------------------------------
// <copyright file="OrganizationConstants.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.Core.Constants
{
    /// <summary>
    /// Fixed identifiers for seeded organization data, shared across module seeders and migrations.
    /// </summary>
    public static class OrganizationConstants
    {
        public static readonly Guid DefaultOrganizationId = Guid.Parse("7a000000-0000-4000-8000-000000000001");

        public static readonly Guid DefaultStoreId = Guid.Parse("51000000-0000-4000-8000-000000000001");

        public static readonly Guid SecondStoreId = Guid.Parse("51000000-0000-4000-8000-000000000002");
    }
}

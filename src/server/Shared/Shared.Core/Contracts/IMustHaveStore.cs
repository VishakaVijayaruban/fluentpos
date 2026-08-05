// --------------------------------------------------------------------------------------------------
// <copyright file="IMustHaveStore.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.Core.Contracts
{
    /// <summary>
    /// Marks an entity as belonging to a single store. Such entities are automatically
    /// filtered by the current tenant's store and stamped with it on insert.
    /// </summary>
    public interface IMustHaveStore
    {
        Guid StoreId { get; set; }
    }
}

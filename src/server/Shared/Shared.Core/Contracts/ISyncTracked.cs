// --------------------------------------------------------------------------------------------------
// <copyright file="ISyncTracked.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;

namespace FluentPOS.Shared.Core.Contracts
{
    /// <summary>
    /// Entities exposed through incremental sync feeds. LastModifiedOn is stamped with the
    /// server clock on every insert/update, so feed cursors never depend on client clocks.
    /// </summary>
    public interface ISyncTracked
    {
        DateTime LastModifiedOn { get; set; }
    }
}

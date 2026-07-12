// --------------------------------------------------------------------------------------------------
// <copyright file="JwtSettings.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace FluentPOS.Modules.Identity.Core.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        // Only disable for local development over plain HTTP.
        public bool RequireHttpsMetadata { get; set; } = true;

        public int TokenExpirationInMinutes { get; set; }

        public int RefreshTokenExpirationInDays { get; set; }
    }
}
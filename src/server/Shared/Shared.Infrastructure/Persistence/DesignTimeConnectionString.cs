// --------------------------------------------------------------------------------------------------
// <copyright file="DesignTimeConnectionString.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    public static class DesignTimeConnectionString
    {
        public static string Read(string startDirectory = null)
        {
            var dir = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var apiDir = Path.Combine(dir.FullName, "API");
                if (Directory.Exists(apiDir) && File.Exists(Path.Combine(apiDir, "appsettings.json")))
                {
                    return new ConfigurationBuilder()
                        .SetBasePath(apiDir)
                        .AddJsonFile("appsettings.json")
                        .Build()["PersistenceSettings:connectionStrings:postgres"];
                }

                dir = dir.Parent;
            }

            throw new InvalidOperationException(
                "Cannot find API/appsettings.json. Run 'dotnet ef' from the Infrastructure project directory.");
        }
    }
}

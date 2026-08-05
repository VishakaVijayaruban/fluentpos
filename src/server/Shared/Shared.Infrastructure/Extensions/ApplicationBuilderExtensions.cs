// --------------------------------------------------------------------------------------------------
// <copyright file="ApplicationBuilderExtensions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.IO;
using System.Runtime.CompilerServices;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Middlewares;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

[assembly: InternalsVisibleTo("FluentPOS.Bootstrapper")]

namespace FluentPOS.Shared.Infrastructure.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSharedInfrastructure(this IApplicationBuilder app)
        {
            // Must run before anything touches the database (the Hangfire dashboard resolves its storage eagerly).
            app.Initialize();

            app.UseMiddleware<GlobalExceptionHandler>();
            app.UseRouting();

            string filesDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            if (!Directory.Exists(filesDirectoryPath))
            {
                Directory.CreateDirectory(filesDirectoryPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = "/files"
            });

            // Offline-first POS client (PWA) hosted by the API during the transition period.
            string posClientPath = Path.Combine(Directory.GetCurrentDirectory(), "PosClient");
            if (Directory.Exists(posClientPath))
            {
                var posFileProvider = new PhysicalFileProvider(posClientPath);
                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = posFileProvider,
                    RequestPath = "/pos"
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = posFileProvider,
                    RequestPath = "/pos"
                });
            }
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                DashboardTitle = "FluentPOS Jobs"
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
                endpoints.MapHealthChecks("/health/ready");
            });
            app.UseSwaggerDocumentation();

            return app;
        }

        internal static IApplicationBuilder Initialize(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var persistenceSettings = serviceScope.ServiceProvider.GetRequiredService<IOptions<PersistenceSettings>>().Value;

            if (persistenceSettings.MigrateOnStartup)
            {
                foreach (var migrator in serviceScope.ServiceProvider.GetServices<IDatabaseMigrator>())
                {
                    migrator.Migrate();
                }
            }

            if (persistenceSettings.SeedOnStartup)
            {
                foreach (var initializer in serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>())
                {
                    initializer.Initialize();
                }
            }

            return app;
        }

        private static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelsExpandDepth(-1);
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
                options.DocExpansion(DocExpansion.None);
            });
            return app;
        }
    }
}
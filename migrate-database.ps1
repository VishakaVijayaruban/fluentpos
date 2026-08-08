#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies all EF Core migrations for every FluentPOS DbContext and creates the database.

.DESCRIPTION
    FluentPOS is a modular monolith: one database, one schema per module, one DbContext per module
    plus a shared application context. This script runs `dotnet ef database update` against each
    context's owning project, using the API as the startup project so appsettings.json connection
    strings (and any environment-variable overrides) are resolved.

    Use this when `PersistenceSettings.MigrateOnStartup` is false -- which it must be whenever more
    than one API replica runs, since concurrent migrators race on the same schema. See
    docs/deployment.md.

    Requires the .NET 10 SDK and the EF CLI:  dotnet tool install --global dotnet-ef

.PARAMETER StartupProject
    Path to the startup project (defaults to src/server/API).

.PARAMETER Context
    Optional: apply only the named context(s), e.g. -Context CatalogDbContext,SalesDbContext.

.EXAMPLE
    .\migrate-database.ps1
    .\migrate-database.ps1 -Context ReportingDbContext
    .\migrate-database.ps1 -StartupProject "src/server/API"
#>
param(
    [string]$StartupProject = "src/server/API",
    [string[]]$Context
)

$ErrorActionPreference = "Stop"

# Every context in the solution. Keep this list in sync with the DbContexts under
# src/server/**/Persistence/ -- a context missing here is a context that never gets migrated.
# Shared first (it owns event logs and extended attributes), then Identity, then the modules.
$modules = @(
    @{ Project = "src/server/Shared/Shared.Infrastructure";                        Context = "ApplicationDbContext"  },
    @{ Project = "src/server/Modules/Identity/Modules.Identity.Infrastructure";     Context = "IdentityDbContext"     },
    @{ Project = "src/server/Modules/Organizations/Modules.Organizations.Infrastructure"; Context = "OrganizationDbContext" },
    @{ Project = "src/server/Modules/Catalog/Modules.Catalog.Infrastructure";       Context = "CatalogDbContext"      },
    @{ Project = "src/server/Modules/People/Modules.People.Infrastructure";         Context = "PeopleDbContext"       },
    @{ Project = "src/server/Modules/Inventory/Modules.Inventory.Infrastructure";   Context = "InventoryDbContext"    },
    @{ Project = "src/server/Modules/Sales/Modules.Sales.Infrastructure";           Context = "SalesDbContext"        },
    @{ Project = "src/server/Modules/Purchasing/Modules.Purchasing.Infrastructure"; Context = "PurchasingDbContext"   },
    @{ Project = "src/server/Modules/Reporting/Modules.Reporting.Infrastructure";   Context = "ReportingDbContext"    }
)

if ($Context) {
    $modules = $modules | Where-Object { $Context -contains $_.Context }
    if (-not $modules) {
        Write-Error "No context matched -Context $($Context -join ', ')."
        exit 1
    }
}

$root = $PSScriptRoot

Write-Host "`n=== FluentPOS Database Migration ===" -ForegroundColor Cyan
Write-Host "Startup project : $StartupProject"
Write-Host "Root            : $root"
Write-Host "Contexts        : $($modules.Count)`n"

$failed = @()

foreach ($module in $modules) {
    $projectPath = Join-Path $root $module.Project
    $context     = $module.Context

    Write-Host "--- $context" -ForegroundColor Yellow
    Write-Host "    Project: $($module.Project)"

    if (-not (Test-Path $projectPath)) {
        Write-Warning "    Project path not found, skipping: $projectPath"
        $failed += $context
        continue
    }

    dotnet ef database update `
        --project $projectPath `
        --startup-project (Join-Path $root $StartupProject) `
        --context $context

    if ($LASTEXITCODE -ne 0) {
        Write-Error "    Migration failed for $context (exit code $LASTEXITCODE)"
        $failed += $context
    } else {
        Write-Host "    Done." -ForegroundColor Green
    }

    Write-Host ""
}

if ($failed.Count -gt 0) {
    Write-Host "=== FAILED contexts ===" -ForegroundColor Red
    $failed | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "`nRerun just that context to isolate it:" -ForegroundColor Red
    Write-Host "  .\migrate-database.ps1 -Context $($failed[0])" -ForegroundColor Red
    Write-Host "For the full EF diagnostic, run dotnet ef directly with --verbose." -ForegroundColor Red
    exit 1
} else {
    Write-Host "=== All migrations applied successfully ===" -ForegroundColor Green
}

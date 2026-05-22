#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies all EF Core migrations for every FluentPOS module and creates the database.

.DESCRIPTION
    Runs `dotnet ef database update` against each module's Infrastructure project,
    using the API as the startup project so appsettings.json connection strings are resolved.

.PARAMETER StartupProject
    Path to the startup project (defaults to src/server/API).

.EXAMPLE
    .\migrate-database.ps1
    .\migrate-database.ps1 -StartupProject "src/server/API"
#>
param(
    [string]$StartupProject = "src/server/API"
)

$ErrorActionPreference = "Stop"

$modules = @(
    @{ Project = "src/server/Modules/Catalog/Modules.Catalog.Infrastructure";   Context = "CatalogDbContext"   },
    @{ Project = "src/server/Modules/Identity/Modules.Identity.Infrastructure"; Context = "IdentityDbContext"  },
    @{ Project = "src/server/Modules/Inventory/Modules.Inventory.Infrastructure"; Context = "InventoryDbContext" },
    @{ Project = "src/server/Modules/People/Modules.People.Infrastructure";     Context = "PeopleDbContext"    },
    @{ Project = "src/server/Modules/Sales/Modules.Sales.Infrastructure";       Context = "SalesDbContext"     }
)

$root = $PSScriptRoot

Write-Host "`n=== FluentPOS Database Migration ===" -ForegroundColor Cyan
Write-Host "Startup project : $StartupProject"
Write-Host "Root            : $root`n"

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
        --context $context `
        --verbose

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
    exit 1
} else {
    Write-Host "=== All migrations applied successfully ===" -ForegroundColor Green
}

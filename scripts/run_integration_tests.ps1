#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all integration tests across all 4 services + the existing DonationService unit tests.

.DESCRIPTION
    Discovers and runs every xUnit test project in the solution using dotnet test.
    No Docker, RabbitMQ, or PostgreSQL required - all tests use EF InMemory + mocked externals.

.PARAMETER Filter
    Optional test filter expression passed to --filter (e.g. "FullyQualifiedName~Auth")

.PARAMETER Project
    Optional: run tests for a single service only. Values: Account, Video, Donation, Chat, DonationUnit

.EXAMPLE
    .\scripts\run_integration_tests.ps1
    .\scripts\run_integration_tests.ps1 -Project Account
    .\scripts\run_integration_tests.ps1 -Filter "FullyQualifiedName~WalletController"
#>
param(
    [string]$Filter = "",
    [ValidateSet("Account", "Video", "Donation", "Chat", "DonationUnit", "")]
    [string]$Project = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# All test projects in the solution
$testProjects = @{
    Account      = "services\Glense.AccountService\IntegrationTests\AccountService.IntegrationTests.csproj"
    Video        = "services\Glense.VideoCatalogue\IntegrationTests\VideoCatalogue.IntegrationTests.csproj"
    Donation     = "Glense.Server\DonationService\IntegrationTests\DonationService.IntegrationTests.csproj"
    Chat         = "services\Glense.ChatService\IntegrationTests\ChatService.IntegrationTests.csproj"
    DonationUnit = "Glense.Server\DonationService\Tests\DonationService.Tests.csproj"
}

# If a specific project was requested, filter to just that one
if ($Project -ne "") {
    $testProjects = @{ $Project = $testProjects[$Project] }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Glense Integration Test Runner" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$totalPassed = 0
$totalFailed = 0
$totalTests  = 0
$failedProjects = @()
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

foreach ($name in $testProjects.Keys | Sort-Object) {
    $csproj = Join-Path $root $testProjects[$name]

    if (-not (Test-Path $csproj)) {
        Write-Host "[SKIP] $name - project not found: $csproj" -ForegroundColor Yellow
        continue
    }

    Write-Host "-------------------------------------------" -ForegroundColor DarkGray
    Write-Host " Running: $name" -ForegroundColor White
    Write-Host "-------------------------------------------" -ForegroundColor DarkGray

    $testArgs = @("test", $csproj, "--verbosity", "normal")
    if ($Filter -ne "") {
        $testArgs += "--filter"
        $testArgs += $Filter
    }

    $output = & dotnet @testArgs 2>&1 | Out-String
    $exitCode = $LASTEXITCODE

    # Parse results from output
    $passedMatch = [regex]::Match($output, 'Passed:\s+(\d+)')
    $failedMatch = [regex]::Match($output, 'Failed:\s+(\d+)')
    $totalMatch  = [regex]::Match($output, 'Total tests:\s+(\d+)')

    $p = if ($passedMatch.Success) { [int]$passedMatch.Groups[1].Value } else { 0 }
    $f = if ($failedMatch.Success) { [int]$failedMatch.Groups[1].Value } else { 0 }
    $t = if ($totalMatch.Success)  { [int]$totalMatch.Groups[1].Value } else { 0 }

    $totalPassed += $p
    $totalFailed += $f
    $totalTests  += $t

    if ($exitCode -eq 0) {
        Write-Host " [PASS] $name - $t tests passed" -ForegroundColor Green
    } else {
        Write-Host " [FAIL] $name - $f/$t tests failed" -ForegroundColor Red
        $failedProjects += $name
        # Print failure details
        $output -split "`n" | Where-Object { $_ -match "Failed |Error Message:|Assert\." } | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
    }
}

$stopwatch.Stop()
$elapsed = $stopwatch.Elapsed.ToString("mm\:ss")

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  RESULTS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Total:   $totalTests" -ForegroundColor White
Write-Host "  Passed:  $totalPassed" -ForegroundColor Green
if ($totalFailed -gt 0) {
    Write-Host "  Failed:  $totalFailed" -ForegroundColor Red
    Write-Host "  Failing: $($failedProjects -join ', ')" -ForegroundColor Red
} else {
    Write-Host "  Failed:  0" -ForegroundColor Green
}
Write-Host "  Time:    $elapsed" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

exit $totalFailed

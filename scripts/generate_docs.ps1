param(
    [switch]$Serve
)

if (-not (Get-Command doxygen -ErrorAction SilentlyContinue)) {
    Write-Error "Doxygen not found. Install Doxygen and ensure 'doxygen' is in PATH."
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    Write-Host "Running Doxygen (config: Doxyfile) from $repoRoot"
    doxygen Doxyfile
    Write-Host "Documentation generated at: docs/doxygen/html"
    if ($Serve) {
        Write-Host "Starting simple HTTP server at http://localhost:8000 (docs/doxygen/html)"
        Push-Location (Join-Path $repoRoot "docs/doxygen/html")
        python -m http.server 8000
    }
} finally {
    Pop-Location
}

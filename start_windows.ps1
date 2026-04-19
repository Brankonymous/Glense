# Thin wrapper — the real logic lives in scripts/start.py
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$py = if (Get-Command py -ErrorAction SilentlyContinue) { "py" }
      elseif (Get-Command python3 -ErrorAction SilentlyContinue) { "python3" }
      elseif (Get-Command python -ErrorAction SilentlyContinue) { "python" }
      else {
          Write-Host "ERROR: Python 3 not found. Install from https://www.python.org/downloads/" -ForegroundColor Red
          exit 1
      }

& $py (Join-Path $PSScriptRoot "scripts\start.py") @args
exit $LASTEXITCODE

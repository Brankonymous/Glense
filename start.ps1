# One-shot script: install prerequisites, start K8s cluster, and seed data
# Usage: .\start.ps1
# Run in PowerShell as Administrator

$ErrorActionPreference = "Stop"

function Log($msg) {
    Write-Host ""
    Write-Host ">>> $msg" -ForegroundColor Cyan
}

function Check-Command($cmd) {
    return [bool](Get-Command $cmd -ErrorAction SilentlyContinue)
}

# ── 1. Check Docker ──────────────────────────────────────────────────────────

Log "Checking Docker..."
if (-not (Check-Command "docker")) {
    Write-Host "ERROR: Docker not found. Download Docker Desktop from https://www.docker.com/products/docker-desktop" -ForegroundColor Red
    exit 1
}
try {
    docker info | Out-Null
} catch {
    Write-Host "ERROR: Docker Desktop is not running. Start it and try again." -ForegroundColor Red
    exit 1
}

# ── 2. Install missing tools via winget ──────────────────────────────────────

Log "Checking prerequisites..."

if (-not (Check-Command "minikube")) {
    Log "Installing minikube..."
    winget install -e --id Kubernetes.minikube
    $env:PATH += ";$env:LOCALAPPDATA\Programs\Kubernetes\Minikube"
}

if (-not (Check-Command "kubectl")) {
    Log "Installing kubectl..."
    winget install -e --id Kubernetes.kubectl
}

if (-not (Check-Command "kompose")) {
    Log "Installing kompose..."
    winget install -e --id Kubernetes.kompose
}

# ── 3. Copy .env if missing ──────────────────────────────────────────────────

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

if (-not (Test-Path ".env")) {
    if (Test-Path ".env.example") {
        Log "Copying .env.example to .env..."
        Copy-Item ".env.example" ".env"
    } else {
        Write-Host "ERROR: No .env or .env.example found." -ForegroundColor Red
        exit 1
    }
}

# ── 4. Start minikube ────────────────────────────────────────────────────────

Log "Starting minikube..."
minikube start

Log "Pointing Docker to minikube's registry..."
& minikube docker-env --shell powershell | Invoke-Expression

# ── 5. Build images ──────────────────────────────────────────────────────────

Log "Building service images..."
docker build -t account-service  services/Glense.AccountService
docker build -t video-service    services/Glense.VideoCatalogue
docker build -t donation-service Glense.Server/DonationService
docker build -t chat-service     services/Glense.ChatService
docker build -t gateway          Glense.Server
docker build -t frontend         glense.client

# ── 6. Apply manifests ───────────────────────────────────────────────────────

Log "Applying Kubernetes manifests..."
kubectl apply -f k8s/ --validate=false

Log "Waiting for all pods to be ready (up to 3 minutes)..."
kubectl wait --for=condition=ready pod --all --timeout=180s

# ── 7. Seed data ─────────────────────────────────────────────────────────────

Log "Seeding test data..."
bash ./scripts/seed.sh

# ── 8. Start port-forward ────────────────────────────────────────────────────

Log "Starting port-forward for gateway on http://localhost:5050..."
Start-Process -NoNewWindow kubectl -ArgumentList "port-forward service/gateway 5050:5050"
Start-Sleep 2

# ── Done ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " Glense is running!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host " Gateway:  http://localhost:5050"
Write-Host " Frontend: http://localhost:3000"
Write-Host ""
Write-Host " Test users (password: Password123!):"
Write-Host "   keki / irena / branko"
Write-Host ""
Write-Host " To stop: minikube stop"
Write-Host " To wipe: minikube delete"
Write-Host "============================================" -ForegroundColor Green

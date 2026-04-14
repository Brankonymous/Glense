#!/usr/bin/env bash
# One-shot script: install prerequisites, start K8s cluster, and seed data
# Usage: ./start.sh

set -e

log() {
    echo ""
    echo ">>> $1"
}

OS="$(uname -s)"

# ── 1. Check Docker ─────────────────────────────────────────────────────────

if ! command -v docker &>/dev/null || ! docker info &>/dev/null 2>&1; then
    echo "ERROR: Docker Desktop is not running. Start it and try again."
    echo "Download: https://www.docker.com/products/docker-desktop"
    exit 1
fi

# ── 2. Install missing tools ────────────────────────────────────────────────

install_tool() {
    local tool=$1
    if command -v "$tool" &>/dev/null; then
        return 0
    fi
    log "Installing $tool..."
    case "$OS" in
        Darwin)
            brew install "$tool"
            ;;
        Linux)
            case "$tool" in
                minikube)
                    curl -LO "https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64"
                    sudo install minikube-linux-amd64 /usr/local/bin/minikube
                    rm minikube-linux-amd64
                    ;;
                kubectl)
                    curl -LO "https://dl.k8s.io/release/$(curl -Ls https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
                    sudo install kubectl /usr/local/bin/kubectl
                    rm kubectl
                    ;;
                kompose)
                    curl -L "https://github.com/kubernetes/kompose/releases/latest/download/kompose-linux-amd64" -o kompose
                    sudo install kompose /usr/local/bin/kompose
                    rm kompose
                    ;;
            esac
            ;;
        *)
            echo "ERROR: Unsupported OS: $OS. Use start.ps1 on Windows."
            exit 1
            ;;
    esac
}

log "Checking prerequisites..."
install_tool minikube
install_tool kubectl
install_tool kompose

# ── 3. Copy .env if missing ─────────────────────────────────────────────────

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        log "Copying .env.example to .env..."
        cp .env.example .env
    else
        echo "ERROR: No .env or .env.example found."
        exit 1
    fi
fi

# ── 4. Start minikube ───────────────────────────────────────────────────────

log "Starting minikube..."
minikube start

log "Pointing Docker to minikube's registry..."
eval $(minikube docker-env)

# ── 5. Build images ─────────────────────────────────────────────────────────

log "Building service images..."
docker build -t account-service  services/Glense.AccountService
docker build -t video-service    services/Glense.VideoCatalogue
docker build -t donation-service Glense.Server/DonationService
docker build -t chat-service     services/Glense.ChatService
docker build -t gateway          Glense.Server
docker build -t frontend         glense.client

# ── 6. Apply manifests ──────────────────────────────────────────────────────

log "Applying Kubernetes manifests..."
kubectl apply -f k8s/ --validate=false

log "Waiting for all pods to be ready (up to 3 minutes)..."
kubectl wait --for=condition=ready pod --all --timeout=180s

# ── 7. Seed data ────────────────────────────────────────────────────────────

log "Seeding test data..."
./scripts/seed.sh

# ── 8. Start port-forward in background ─────────────────────────────────────

log "Starting port-forward for gateway on http://localhost:5050..."
pkill -f "port-forward service/gateway" 2>/dev/null || true
kubectl port-forward service/gateway 5050:5050 &
sleep 2

# ── Done ────────────────────────────────────────────────────────────────────

echo ""
echo "============================================"
echo " Glense is running!"
echo "============================================"
echo " Gateway:  http://localhost:5050"
echo " Frontend: http://localhost:3000"
echo ""
echo " Test users (password: Password123!):"
echo "   keki / irena / branko"
echo ""
echo " To stop: minikube stop"
echo " To wipe: minikube delete"
echo "============================================"

#!/usr/bin/env bash
# Deploy Glense to local minikube cluster

# Function to log messages
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}

# Run a command, log the result, and do not exit immediately on failure
run_step() {
    local description="$1"
    shift
    log "=== $description ==="
    "$@"
    local status=$?
    if [ $status -ne 0 ]; then
        log "ERROR: $description failed with exit code $status"
    else
        log "SUCCESS: $description"
    fi
    return $status
}

log "NOTE: This script will continue after failures so you can inspect errors."

run_step "Starting minikube" minikube start
run_step "Pointing Docker to minikube's registry" eval "$(minikube docker-env)"

log "=== Building service images inside minikube ==="
run_step "Building account-service" docker build -t account-service ../services/Glense.AccountService
run_step "Building video-service" docker build -t video-service ../services/Glense.VideoCatalogue
run_step "Building donation-service" docker build -t donation-service ../Glense.Server/DonationService
run_step "Building chat-service" docker build -t chat-service ../services/Glense.ChatService
run_step "Building gateway" docker build -t gateway ../Glense.Server
run_step "Building frontend" docker build -t frontend ../glense.client

run_step "Applying Kubernetes manifests" kubectl apply -f . --validate=false
run_step "Waiting for pods to be ready" kubectl wait --for=condition=ready pod --all --timeout=120s

log "=== Done! ==="
log "Run this to access the gateway:"
log "  kubectl port-forward service/gateway 5050:5050"

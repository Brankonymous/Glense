#!/usr/bin/env bash
# Deploy Glense to local minikube cluster
set -e

echo "=== Starting minikube ==="
minikube start

echo "=== Pointing Docker to minikube's registry ==="
eval $(minikube docker-env)

echo "=== Building service images inside minikube ==="
docker build -t account-service   ../services/Glense.AccountService
docker build -t video-service     ../services/Glense.VideoCatalogue
docker build -t donation-service  ../Glense.Server/DonationService
docker build -t chat-service      ../services/Glense.ChatService
docker build -t gateway           ../Glense.Server

echo "=== Applying manifests ==="
kubectl apply -f .

echo "=== Waiting for pods to be ready ==="
kubectl wait --for=condition=ready pod --all --timeout=120s

echo "=== Done! ==="
echo "Run this to access the gateway:"
echo "  kubectl port-forward service/gateway 5050:5050"

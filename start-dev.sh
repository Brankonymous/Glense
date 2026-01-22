#!/bin/bash

# Glense Development Startup Script
# Starts all services needed for local development

set -e

echo "🚀 Starting Glense Development Environment..."
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to check if a port is in use
check_port() {
    lsof -i :$1 >/dev/null 2>&1
}

# Function to wait for a service to be ready
wait_for_service() {
    local url=$1
    local name=$2
    local max_attempts=30
    local attempt=1

    echo -n "  Waiting for $name..."
    while ! curl -s "$url" >/dev/null 2>&1; do
        if [ $attempt -ge $max_attempts ]; then
            echo -e " ${RED}FAILED${NC}"
            return 1
        fi
        sleep 1
        attempt=$((attempt + 1))
        echo -n "."
    done
    echo -e " ${GREEN}OK${NC}"
}

# Stop any existing docker containers that might conflict
echo "📦 Setting up Docker services..."
docker stop glense_account_service 2>/dev/null || true

# Start PostgreSQL via docker-compose
echo "  Starting PostgreSQL..."
cd "$SCRIPT_DIR"
docker-compose up postgres_account -d 2>/dev/null
sleep 2

# Check if PostgreSQL is ready
echo -n "  Waiting for PostgreSQL..."
max_attempts=30
attempt=1
while ! docker exec glense_postgres_account pg_isready -U glense -d glense_account >/dev/null 2>&1; do
    if [ $attempt -ge $max_attempts ]; then
        echo -e " ${RED}FAILED${NC}"
        echo "PostgreSQL failed to start. Check docker logs."
        exit 1
    fi
    sleep 1
    attempt=$((attempt + 1))
    echo -n "."
done
echo -e " ${GREEN}OK${NC}"

echo ""
echo "🔧 Starting Microservices..."

# Start Account Service
echo "  Starting Account Service (port 5001)..."
cd "$SCRIPT_DIR/services/Glense.AccountService"
dotnet run --urls "http://localhost:5001" > /tmp/account-service.log 2>&1 &
ACCOUNT_PID=$!
echo "    PID: $ACCOUNT_PID"

# Start Donation Service
echo "  Starting Donation Service (port 5100)..."
cd "$SCRIPT_DIR/Glense.Server/DonationService"
dotnet run > /tmp/donation-service.log 2>&1 &
DONATION_PID=$!
echo "    PID: $DONATION_PID"

# Start Gateway
echo "  Starting Gateway (port 5050)..."
cd "$SCRIPT_DIR/Glense.Server"
dotnet run --urls "http://localhost:5050" > /tmp/gateway.log 2>&1 &
GATEWAY_PID=$!
echo "    PID: $GATEWAY_PID"

# Wait for services to be ready
echo ""
echo "⏳ Waiting for services to be ready..."
sleep 3

wait_for_service "http://localhost:5001/health" "Account Service"
wait_for_service "http://localhost:5100/health" "Donation Service"
wait_for_service "http://localhost:5050/health" "Gateway"

# Start Frontend
echo ""
echo "🌐 Starting Frontend (port 5173)..."
cd "$SCRIPT_DIR/glense.client"
npm run dev > /tmp/frontend.log 2>&1 &
FRONTEND_PID=$!
echo "    PID: $FRONTEND_PID"

sleep 3

echo ""
echo -e "${GREEN}✅ All services started!${NC}"
echo ""
echo "📍 Service URLs:"
echo "   Frontend:        http://localhost:5173"
echo "   Gateway:         http://localhost:5050"
echo "   Account Service: http://localhost:5001"
echo "   Donation Service: http://localhost:5100"
echo ""
echo "📋 Logs:"
echo "   Account Service: /tmp/account-service.log"
echo "   Donation Service: /tmp/donation-service.log"
echo "   Gateway:         /tmp/gateway.log"
echo "   Frontend:        /tmp/frontend.log"
echo ""
echo "🛑 To stop all services, run: ./stop-dev.sh"
echo ""

# Save PIDs to file for stop script
echo "$ACCOUNT_PID" > /tmp/glense-pids.txt
echo "$DONATION_PID" >> /tmp/glense-pids.txt
echo "$GATEWAY_PID" >> /tmp/glense-pids.txt
echo "$FRONTEND_PID" >> /tmp/glense-pids.txt

# Keep script running and show combined logs
echo "📜 Showing combined logs (Ctrl+C to stop viewing, services will keep running)..."
echo "==========================================================================="
tail -f /tmp/account-service.log /tmp/donation-service.log /tmp/gateway.log /tmp/frontend.log 2>/dev/null

#!/bin/bash

# Glense Development Stop Script
# Stops all development services

echo "🛑 Stopping Glense Development Environment..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m'

# Kill processes from PID file
if [ -f /tmp/glense-pids.txt ]; then
    while read pid; do
        if kill -0 "$pid" 2>/dev/null; then
            kill "$pid" 2>/dev/null && echo "  Stopped process $pid"
        fi
    done < /tmp/glense-pids.txt
    rm /tmp/glense-pids.txt
fi

# Kill any remaining dotnet processes for our services
pkill -f "Glense.AccountService" 2>/dev/null && echo "  Stopped Account Service"
pkill -f "DonationService" 2>/dev/null && echo "  Stopped Donation Service"
pkill -f "Glense.Server" 2>/dev/null && echo "  Stopped Gateway"

# Kill frontend dev server
pkill -f "vite.*glense" 2>/dev/null && echo "  Stopped Frontend"

# Optionally stop PostgreSQL container
read -p "Stop PostgreSQL container? (y/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker stop glense_postgres_account 2>/dev/null && echo "  Stopped PostgreSQL"
fi

echo ""
echo -e "${GREEN}✅ All services stopped${NC}"

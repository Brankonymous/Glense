#!/usr/bin/env bash
# Glense Development Environment (Container-based)
# Usage:
#   ./dev.sh          Start everything + seed data
#   ./dev.sh up       Start everything + seed data
#   ./dev.sh down     Stop everything
#   ./dev.sh restart  Full restart + seed data
#   ./dev.sh logs     Follow all container logs
#   ./dev.sh seed     Seed test data only

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Auto-detect container runtime
if command -v podman-compose &> /dev/null; then
    COMPOSE="podman-compose"
    RUNTIME="podman"
elif command -v podman &> /dev/null && podman compose version &> /dev/null; then
    COMPOSE="podman compose"
    RUNTIME="podman"
elif command -v docker &> /dev/null; then
    COMPOSE="docker compose"
    RUNTIME="docker"
else
    echo "ERROR: No container runtime found (podman or docker)"
    exit 1
fi

echo "Using: $COMPOSE"

GATEWAY_URL="http://localhost:5050"
SERVICES="postgres_account postgres_video postgres_donation postgres_chat account_service video_service donation_service chat_service gateway"

kill_stale_dotnet() {
    echo "Checking for stale dotnet processes on service ports..."
    local killed=0
    for port in 5050 5001 5002 5100 5004; do
        local pids
        pids=$(lsof -ti :$port 2>/dev/null || true)
        if [ -n "$pids" ]; then
            for pid in $pids; do
                local pname
                pname=$(ps -p "$pid" -o comm= 2>/dev/null || true)
                # Only kill dotnet/Glense processes, not container runtime
                if echo "$pname" | grep -qiE "dotnet|glense"; then
                    kill "$pid" 2>/dev/null && echo "  Killed stale process $pname (PID $pid) on port $port" && killed=1
                fi
            done
        fi
    done
    [ $killed -eq 0 ] && echo "  None found"
}

wait_for_health() {
    local url=$1 name=$2 max=60 attempt=0
    printf "  Waiting for %-20s" "$name..."
    while ! curl -sf "$url" > /dev/null 2>&1; do
        if [ $attempt -ge $max ]; then
            echo " FAILED (timeout after ${max}s)"
            echo "  Check logs: $RUNTIME logs glense_${name// /_}"
            return 1
        fi
        sleep 1
        attempt=$((attempt + 1))
    done
    echo " OK (${attempt}s)"
}

do_up() {
    kill_stale_dotnet
    echo ""
    echo "Starting containers..."
    $COMPOSE up --build -d $SERVICES 2>&1 | tail -5

    echo ""
    echo "Waiting for services to be healthy..."
    wait_for_health "$GATEWAY_URL/health" "gateway"
    wait_for_health "http://localhost:5001/health" "account_service"
    wait_for_health "http://localhost:5002/health" "video_service"
    wait_for_health "http://localhost:5100/health" "donation_service"
    wait_for_health "http://localhost:5004/health" "chat_service"

    echo ""
    do_seed

    echo ""
    echo "Ready!"
    echo "  Gateway:  $GATEWAY_URL"
    echo "  Frontend: cd glense.client && npm run dev"
    echo ""
    echo "  Logs:     ./dev.sh logs"
    echo "  Stop:     ./dev.sh down"
}

do_down() {
    kill_stale_dotnet
    echo ""
    echo "Stopping containers..."
    $COMPOSE down 2>&1 | tail -5
    echo "Done"
}

do_seed() {
    echo "Seeding test data..."
    bash "$SCRIPT_DIR/scripts/seed.sh" "$GATEWAY_URL"
}

do_logs() {
    local target=${1:-}
    if [ -n "$target" ]; then
        $RUNTIME logs -f "glense_$target"
    else
        # podman-compose doesn't support multi-container logs remotely
        echo "Tailing all service logs (Ctrl+C to stop)..."
        local pids=()
        for c in gateway account_service video_service donation_service chat_service; do
            $RUNTIME logs -f "glense_$c" 2>&1 | sed "s/^/[$c] /" &
            pids+=($!)
        done
        trap "kill ${pids[*]} 2>/dev/null; exit" INT TERM
        wait
    fi
}

do_prune() {
    do_down
    echo ""
    echo "Pruning all container images and build cache..."
    $RUNTIME system prune -a -f
    echo "Done"
}

do_nuke() {
    echo "Stopping ALL containers..."
    $RUNTIME stop -a 2>/dev/null
    echo "Removing ALL containers..."
    $RUNTIME rm -a -f 2>/dev/null
    echo "Removing ALL volumes..."
    $RUNTIME volume prune -f 2>/dev/null
    echo "Removing ALL images..."
    $RUNTIME rmi -a -f 2>/dev/null
    echo ""
    echo "Everything wiped. Run './dev.sh up' to start fresh."
}

do_reset() {
    echo "Full reset: nuke + rebuild + seed"
    do_nuke
    echo ""
    do_up
}

case "${1:-up}" in
    up)       do_up ;;
    down)     do_down ;;
    restart)  do_down; echo ""; do_up ;;
    logs)     do_logs "$2" ;;
    seed)     do_seed ;;
    prune)    do_prune ;;
    nuke)     do_nuke ;;
    reset)    do_reset ;;
    *)        echo "Usage: ./dev.sh [up|down|restart|logs|seed|nuke|reset|prune]" ;;
esac

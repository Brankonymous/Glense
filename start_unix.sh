#!/usr/bin/env bash
# Thin wrapper — the real logic lives in scripts/start.py
set -e
cd "$(dirname "$0")"

if ! command -v python3 &>/dev/null; then
    echo "ERROR: python3 not found. Install Python 3 from https://www.python.org/downloads/" >&2
    exit 1
fi

exec python3 scripts/start.py "$@"

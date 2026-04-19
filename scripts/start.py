#!/usr/bin/env python3
"""Start the Glense K8s local dev cluster.

Builds images, applies manifests, starts port-forwards, and seeds test data.
Works identically on macOS, Linux, and Windows.
"""

import os
import platform
import shutil
import socket
import subprocess
import sys
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent

# (image tag, build context) — built with docker against minikube's daemon.
IMAGES = [
    ("account-service",  "services/Glense.AccountService"),
    ("video-service",    "services/Glense.VideoCatalogue"),
    ("donation-service", "Glense.Server/DonationService"),
    ("chat-service",     "services/Glense.ChatService"),
    ("gateway",          "Glense.Server"),
    ("frontend",         "glense.client"),
]


def log(msg):
    print(f"\n>>> {msg}", flush=True)


def bail(msg):
    print(f"\nERROR: {msg}", file=sys.stderr)
    sys.exit(1)


def have(cmd):
    return shutil.which(cmd) is not None


def install_hint(tool):
    system = platform.system()
    if system == "Darwin":
        return f"brew install {tool}"
    if system == "Windows":
        return f"winget install -e --id Kubernetes.{tool} --accept-source-agreements --accept-package-agreements"
    return f"see https://minikube.sigs.k8s.io/docs/start/ (or your distro's package manager) for {tool}"


# ── 1. Docker ───────────────────────────────────────────────────────────────

log("Checking Docker...")
if not have("docker"):
    bail("docker not found. Install Docker Desktop: https://www.docker.com/products/docker-desktop")

if subprocess.run(["docker", "info"], capture_output=True).returncode != 0:
    bail("Docker Desktop is not running. Start it and try again.")


# ── 2. Required tools ───────────────────────────────────────────────────────

log("Checking prerequisites...")
missing = [t for t in ("minikube", "kubectl", "kompose") if not have(t)]
if missing:
    msg = "Missing tools: " + ", ".join(missing) + "\n  Install:"
    for t in missing:
        msg += f"\n    {t}: {install_hint(t)}"
    bail(msg)


# ── 3. .env ─────────────────────────────────────────────────────────────────

os.chdir(ROOT)
if not Path(".env").exists():
    if Path(".env.example").exists():
        log("Copying .env.example to .env...")
        shutil.copy(".env.example", ".env")
    else:
        bail("No .env or .env.example found.")


# ── 4. minikube ─────────────────────────────────────────────────────────────

log("Starting minikube...")
if subprocess.run(["minikube", "start"]).returncode != 0:
    bail("minikube start failed. Try: minikube delete --all --purge; "
         "raise Docker Desktop memory to 6GB+; then retry.")


# ── 5. Point docker at minikube's daemon ────────────────────────────────────

log("Pointing Docker at minikube's daemon...")
r = subprocess.run(["minikube", "docker-env", "--shell", "bash"],
                   capture_output=True, text=True, check=True)
for line in r.stdout.splitlines():
    line = line.strip()
    if not line.startswith("export "):
        continue
    kv = line[len("export "):]
    if "=" not in kv:
        continue
    k, v = kv.split("=", 1)
    os.environ[k] = v.strip('"').strip("'")


# ── 6. Build images ─────────────────────────────────────────────────────────

log("Building service images...")
for tag, path in IMAGES:
    print(f"  - {tag} from {path}")
    r = subprocess.run(["docker", "build", "-q", "-t", tag, path])
    if r.returncode != 0:
        bail(f"docker build failed for {tag}")


# ── 7. Apply manifests ──────────────────────────────────────────────────────

log("Applying Kubernetes manifests...")
subprocess.run(["kubectl", "apply", "-f", "k8s/", "--validate=false"], check=True)


# ── 8. Wait for pods ────────────────────────────────────────────────────────

log("Waiting for pods to be Ready (up to 5 minutes)...")
subprocess.run(["kubectl", "wait", "--for=condition=ready",
                "pod", "--all", "--timeout=300s"])


# ── 9. Port-forwards ────────────────────────────────────────────────────────

log("Starting port-forwards...")

def kill_existing_port_forwards():
    if platform.system() == "Windows":
        subprocess.run([
            "powershell", "-NoProfile", "-Command",
            "Get-CimInstance Win32_Process -Filter \"Name='kubectl.exe'\" "
            "-ErrorAction SilentlyContinue | "
            "Where-Object { $_.CommandLine -match 'port-forward' } | "
            "ForEach-Object { Stop-Process -Id $_.ProcessId -Force "
            "-ErrorAction SilentlyContinue }"
        ], capture_output=True)
    else:
        subprocess.run(["pkill", "-f", "kubectl.*port-forward"], capture_output=True)


def start_port_forward(service, local, remote):
    return subprocess.Popen(
        ["kubectl", "port-forward", f"service/{service}", f"{local}:{remote}"],
        stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
    )


kill_existing_port_forwards()
start_port_forward("gateway", 5050, 5050)
start_port_forward("frontend", 3000, 80)


# ── 10. Wait for gateway TCP ────────────────────────────────────────────────

print("  Waiting for gateway ", end="", flush=True)
for _ in range(30):
    try:
        with socket.create_connection(("127.0.0.1", 5050), timeout=1):
            print(" ready")
            break
    except OSError:
        print(".", end="", flush=True)
        time.sleep(1)
else:
    print(" TIMEOUT")


# ── 11. Wait for MassTransit consumers to bind ──────────────────────────────
# Without this, the first "user registered" events can be published before
# donation-service has bound its consumer queue, and RabbitMQ silently drops
# them. Once the queue is listed, subsequent events are routed correctly.

print("  Waiting for RabbitMQ consumers ", end="", flush=True)
for _ in range(60):
    r = subprocess.run(
        ["kubectl", "exec", "deployment/rabbitmq", "--",
         "rabbitmqctl", "list_queues", "name"],
        capture_output=True, text=True,
    )
    if "UserRegisteredEvent" in r.stdout:
        print(" ready")
        break
    print(".", end="", flush=True)
    time.sleep(1)
else:
    print(" TIMEOUT")


# ── 12. Seed ────────────────────────────────────────────────────────────────

log("Seeding test data...")
subprocess.run([sys.executable, str(ROOT / "scripts" / "seed.py")])


# ── Done ────────────────────────────────────────────────────────────────────

print("\n" + "=" * 44)
print(" Glense is running!")
print("=" * 44)
print(" Gateway:  http://localhost:5050")
print(" Frontend: http://localhost:3000")
print()
print(" Test users (password: Password123!):")
print("   keki / irena / branko")
print()
print(" To stop: minikube stop")
print(" To wipe: minikube delete")
print("=" * 44)

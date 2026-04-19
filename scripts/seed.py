#!/usr/bin/env python3
"""Seed test data into the local Glense K8s cluster.

Usage: python3 scripts/seed.py [GATEWAY_URL]
Default GATEWAY_URL is http://localhost:5050
"""

import json
import random
import subprocess
import sys
import time
import urllib.error
import urllib.request
import uuid


GATEWAY = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5050"


def http(method, url, body=None, token=None, timeout=10):
    headers = {}
    data = None
    if body is not None:
        headers["Content-Type"] = "application/json"
        data = json.dumps(body).encode("utf-8")
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        raw = resp.read().decode("utf-8")
        return json.loads(raw) if raw else None


def pg_exec(label, container, db, sql, silent=False):
    """Run SQL against a postgres instance — tries kubectl first, then docker."""
    # Try K8s
    pod = subprocess.run(
        ["kubectl", "get", "pod",
         "-l", f"io.kompose.service={label}",
         "-o", "jsonpath={.items[0].metadata.name}"],
        capture_output=True, text=True,
    ).stdout.strip()

    if pod:
        cmd = ["kubectl", "exec", "-i", pod, "--",
               "psql", "-U", "glense", "-d", db]
    else:
        # Fallback: docker-compose container
        cmd = ["docker", "exec", "-i", container,
               "psql", "-U", "glense", "-d", db]

    r = subprocess.run(cmd, input=sql, capture_output=True, text=True)
    if r.returncode != 0 and not silent:
        raise RuntimeError(r.stderr.strip() or "psql failed")
    return r.returncode == 0


# ── Clean databases (silent if empty or not reachable) ────────────────────────

print("=== Cleaning all databases ===")
TRUNCATE_SQL = """
DO $$
DECLARE r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'TRUNCATE TABLE "' || r.tablename || '" CASCADE';
    END LOOP;
END $$;
"""

DBS = [
    # (k8s label,        docker-compose container,   db name)
    ("postgres-account",  "glense_postgres_account",  "glense_account"),
    ("postgres-video",    "glense_postgres_video",    "glense_video"),
    ("postgres-donation", "glense_postgres_donation", "glense_donation"),
    ("postgres-chat",     "glense_postgres_chat",     "glense_chat"),
]

for label, container, db in DBS:
    if pg_exec(label, container, db, TRUNCATE_SQL, silent=True):
        print(f"  Cleaned {db}")
    else:
        print(f"  Skipped {db}")


# ── Register users and top up wallets ────────────────────────────────────────

print("\n=== Seeding test users ===")

def register_or_login(username, email, account_type):
    body = {
        "username": username,
        "email": email,
        "password": "Password123!",
        "confirmPassword": "Password123!",
        "accountType": account_type,
    }
    try:
        resp = http("POST", f"{GATEWAY}/api/auth/register", body=body)
        print(f"  Created {username} ({account_type}) -> {resp['user']['id']}")
        return resp["user"]["id"], resp["token"]
    except urllib.error.HTTPError as e:
        # Already registered? try logging in to get a fresh token
        try:
            resp = http("POST", f"{GATEWAY}/api/auth/login",
                        body={"usernameOrEmail": username, "password": "Password123!"})
            print(f"  {username} already exists -> {resp['user']['id']}")
            return resp["user"]["id"], resp["token"]
        except urllib.error.HTTPError:
            print(f"  {username}: could not register or log in (HTTP {e.code})")
            return None, None


users = {}
USERS = [
    ("keki",   "keki@glense.test",   "creator"),
    ("irena",  "irena@glense.test",  "creator"),
    ("branko", "branko@glense.test", "user"),
]

for name, email, kind in USERS:
    uid, token = register_or_login(name, email, kind)
    users[name] = {"id": uid, "token": token}
    if not uid or not token:
        continue
    # Wallet is created asynchronously by donation-service consuming a
    # UserRegisteredEvent. Retry briefly until the wallet exists.
    for _ in range(30):
        try:
            http("POST", f"{GATEWAY}/api/wallet/user/{uid}/topup",
                 body={"amount": 500}, token=token)
            print("    Wallet topped up with $500")
            break
        except urllib.error.HTTPError:
            time.sleep(1)
    else:
        print("    WARNING: could not top up wallet after 30s")


# ── Donations ────────────────────────────────────────────────────────────────

print("\n=== Seeding sample donations ===")

DONATIONS = [
    ("branko", "keki",   25, "Great content, keep it up!"),
    ("branko", "irena",  10, "Love your streams!"),
    ("keki",   "branko", 50, "Thanks for the support!"),
    ("irena",  "keki",   15, "Collab soon?"),
]

for donor_name, recipient_name, amount, message in DONATIONS:
    donor = users[donor_name]
    recipient = users[recipient_name]
    if not (donor["id"] and recipient["id"] and donor["token"]):
        print(f"  Skipped {donor_name} -> {recipient_name}")
        continue
    try:
        http("POST", f"{GATEWAY}/api/donation",
             body={
                 "donorUserId":     donor["id"],
                 "recipientUserId": recipient["id"],
                 "amount":          amount,
                 "message":         message,
             },
             token=donor["token"])
        print(f"  {donor_name} -> {recipient_name}: ${amount} ({message})")
    except urllib.error.HTTPError as e:
        print(f"  FAILED {donor_name} -> {recipient_name}: HTTP {e.code}")


# ── Videos and comments ──────────────────────────────────────────────────────

print("\n=== Seeding videos & comments ===")

uids = [users["keki"]["id"], users["irena"]["id"], users["branko"]["id"]]
names = ["keki", "irena", "branko"]

if None in uids:
    print("  Skipping video seed (missing user IDs)")
    sys.exit(0)

random.seed(42)
VIDEOS = [
    ("Microservices Explained in 5 Minutes", "Quick overview of microservice architecture patterns", "lL_j7ilk7rc", 320000, 15000, 200, "Education"),
    ("Docker in 100 Seconds", "Everything you need to know about Docker, fast", "Gjnup-PuquQ", 890000, 42000, 300, "Education"),
    ("How Do APIs Work?", "APIs explained with real-world examples", "s7wmiS2mSXY", 234175, 12300, 40, "Education"),
    ("Build and Deploy 5 JavaScript and React API Projects", "Full course covering 5 real-world API projects", "GDa8kZLNhJ4", 54321, 4560, 10, "Education"),
    (".NET 8 Full Course for Beginners", "Complete beginner guide to .NET 8 and C#", "AhAxLiGC7Pc", 98000, 5600, 30, "Education"),
    ("Node.js Ultimate Beginners Guide", "Learn Node.js from scratch in this crash course", "ENrzD9HAZK4", 445000, 21000, 1800, "Podcast"),
    ("PostgreSQL Tutorial for Beginners", "Learn PostgreSQL from the ground up", "SpfIwlAYaKk", 187000, 9800, 120, "Education"),
    ("Git and GitHub for Beginners", "Full crash course on Git and GitHub", "RGOj5yH7evk", 150000, 8700, 95, "Education"),
]

COMMENTS_LIST = [
    "This is amazing content, keep it up!",
    "Finally someone explains this properly",
    "Great video, learned a lot!",
    "I have been waiting for this video",
    "Can you do a follow-up on this topic?",
    "This changed my perspective completely",
    "Subscribed! More content like this please",
    "The production quality is insane",
    "Watching this at 2am, no regrets",
    "This deserves way more views",
    "Thanks for sharing your knowledge",
    "Bookmarked for later reference",
]

sql_parts = []
video_ids = []

for i, (title, desc, ytid, views, likes, dislikes, cat) in enumerate(VIDEOS):
    vid = str(uuid.uuid4())
    video_ids.append(vid)
    uid = uids[i % 3]
    days = i * 7 + 1
    sql_parts.append(
        f'INSERT INTO "Videos" (id, title, description, upload_date, uploader_id, '
        f'thumbnail_url, video_url, view_count, like_count, dislike_count, category) '
        f"VALUES ('{vid}', '{title}', '{desc}', NOW() - interval '{days} days', '{uid}', "
        f"'https://img.youtube.com/vi/{ytid}/hqdefault.jpg', "
        f"'https://www.youtube.com/watch?v={ytid}', "
        f"{views}, {likes}, {dislikes}, '{cat}');"
    )

for vid in video_ids:
    for j in range(3):
        cid = str(uuid.uuid4())
        ni = (abs(hash(vid)) + j) % 3
        ci = (abs(hash(vid)) + j) % len(COMMENTS_LIST)
        lc = random.randint(0, 200)
        dc = random.randint(0, 20)
        hrs = random.randint(1, 720)
        sql_parts.append(
            f'INSERT INTO "Comments" (id, video_id, user_id, username, content, '
            f'like_count, dislike_count, created_at) '
            f"VALUES ('{cid}', '{vid}', '{uids[ni]}', '{names[ni]}', "
            f"'{COMMENTS_LIST[ci]}', {lc}, {dc}, NOW() - interval '{hrs} hours');"
        )

try:
    pg_exec("postgres-video", "glense_postgres_video", "glense_video", "\n".join(sql_parts))
    print(f"  Inserted {len(VIDEOS)} videos with comments")
except Exception as e:
    print(f"  ERROR: {e}")

print("\n=== Done! ===")
print("All users have password: Password123!")
print("Frontend: http://localhost:3000")

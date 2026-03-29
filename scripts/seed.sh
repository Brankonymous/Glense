#!/usr/bin/env bash
# Seed all test data for local development
# Usage: ./scripts/seed.sh [GATEWAY_URL]
# Examples:
#   ./scripts/seed.sh                          # defaults to http://localhost:5050
#   ./scripts/seed.sh http://localhost:5050     # explicit gateway URL

GATEWAY=${1:-http://localhost:5050}
ACCOUNT=$GATEWAY
DONATION=$GATEWAY
VIDEO=$GATEWAY

# Auto-detect container runtime (podman or docker)
if command -v podman &> /dev/null; then
    CONTAINER_CMD=podman
elif command -v docker &> /dev/null; then
    CONTAINER_CMD=docker
else
    echo "ERROR: Neither podman nor docker found in PATH"
    exit 1
fi
echo "Using container runtime: $CONTAINER_CMD"

PG_VIDEO=${PG_VIDEO:-glense_postgres_video}

# ── Clean all databases before seeding ──
echo "=== Cleaning all databases ==="

clean_db() {
    local container=$1 db=$2
    echo "  Cleaning $db ($container)..."
    $CONTAINER_CMD exec -i "$container" psql -U glense -d "$db" -c "
        DO \$\$
        DECLARE r RECORD;
        BEGIN
            FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                EXECUTE 'TRUNCATE TABLE \"' || r.tablename || '\" CASCADE';
            END LOOP;
        END \$\$;
    " > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "    Done"
    else
        echo "    WARNING: Failed to clean $db (is $container running?)"
    fi
}

clean_db "glense_postgres_account" "glense_account"
clean_db "glense_postgres_video"   "glense_video"
clean_db "glense_postgres_donation" "glense_donation"
clean_db "glense_postgres_chat"    "glense_chat"

echo ""
echo "=== Seeding test users ==="

register_or_find() {
    local username=$1 email=$2 type=$3
    local result user_id

    result=$(curl -s -X POST "$ACCOUNT/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"email\": \"$email\",
            \"password\": \"Password123!\",
            \"confirmPassword\": \"Password123!\",
            \"accountType\": \"$type\"
        }")

    user_id=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin)['user']['id'])" 2>/dev/null)

    if [ -n "$user_id" ]; then
        echo "  Created $username ($type) -> $user_id" >&2
        curl -s -X POST "$DONATION/api/wallet/user/$user_id/topup" \
            -H "Content-Type: application/json" -d '{"amount": 500}' > /dev/null 2>&1
        echo "    Wallet topped up with \$500" >&2
    else
        user_id=$(curl -s "$ACCOUNT/api/profile/search?q=$username" | \
            python3 -c "import sys,json; [print(u['id']) for u in json.load(sys.stdin) if u['username']=='$username']" 2>/dev/null)
        if [ -n "$user_id" ]; then
            echo "  $username already exists -> $user_id" >&2
        else
            echo "  $username: could not register or find" >&2
        fi
    fi

    echo "$user_id"
}

KEKI_ID=$(register_or_find "keki" "keki@glense.test" "creator")
IRENA_ID=$(register_or_find "irena" "irena@glense.test" "creator")
BRANKO_ID=$(register_or_find "branko" "branko@glense.test" "user")

echo ""
echo "=== Seeding sample donations ==="

send_donation() {
    local from_name=$1 from_id=$2 to_name=$3 to_id=$4 amount=$5 message=$6
    if [ -z "$from_id" ] || [ -z "$to_id" ]; then
        echo "  Skipping $from_name -> $to_name (missing ID)"
        return
    fi
    curl -s -X POST "$DONATION/api/donation" \
        -H "Content-Type: application/json" \
        -d "{\"donorUserId\":\"$from_id\",\"recipientUserId\":\"$to_id\",\"amount\":$amount,\"message\":\"$message\"}" > /dev/null 2>&1
    echo "  $from_name -> $to_name: \$$amount ($message)"
}

send_donation "branko" "$BRANKO_ID" "keki"   "$KEKI_ID"   25 "Great content, keep it up!"
send_donation "branko" "$BRANKO_ID" "irena"  "$IRENA_ID"  10 "Love your streams!"
send_donation "keki"   "$KEKI_ID"   "branko" "$BRANKO_ID" 50 "Thanks for the support!"
send_donation "irena"  "$IRENA_ID"  "keki"   "$KEKI_ID"   15 "Collab soon?"

echo ""
echo "=== Seeding videos & comments ==="

TMPFILE=$(mktemp)
python3 - "$KEKI_ID" "$IRENA_ID" "$BRANKO_ID" > "$TMPFILE" << 'PYEOF'
import uuid, random, sys
random.seed(42)

uids = sys.argv[1:4]
names = ['keki', 'irena', 'branko']

videos = [
    ('Microservices Explained in 5 Minutes', 'Quick overview of microservice architecture patterns', 'lL_j7ilk7rc', 320000, 15000, 200, 'Education'),
    ('Docker in 100 Seconds', 'Everything you need to know about Docker, fast', 'Gjnup-PuquQ', 890000, 42000, 300, 'Education'),
    ('How Do APIs Work?', 'APIs explained with real-world examples', 's7wmiS2mSXY', 234175, 12300, 40, 'Education'),
    ('Build and Deploy 5 JavaScript and React API Projects', 'Full course covering 5 real-world API projects', 'GDa8kZLNhJ4', 54321, 4560, 10, 'Education'),
    ('.NET 8 Full Course for Beginners', 'Complete beginner guide to .NET 8 and C#', 'AhAxLiGC7Pc', 98000, 5600, 30, 'Education'),
    ('Node.js Ultimate Beginners Guide', 'Learn Node.js from scratch in this crash course', 'ENrzD9HAZK4', 445000, 21000, 1800, 'Podcast'),
    ('PostgreSQL Tutorial for Beginners', 'Learn PostgreSQL from the ground up', 'SpfIwlAYaKk', 187000, 9800, 120, 'Education'),
    ('Git and GitHub for Beginners', 'Full crash course on Git and GitHub', 'RGOj5yH7evk', 150000, 8700, 95, 'Education'),
]

comments_list = [
    'This is amazing content, keep it up!',
    'Finally someone explains this properly',
    'Great video, learned a lot!',
    'I have been waiting for this video',
    'Can you do a follow-up on this topic?',
    'This changed my perspective completely',
    'Subscribed! More content like this please',
    'The production quality is insane',
    'Watching this at 2am, no regrets',
    'This deserves way more views',
    'Thanks for sharing your knowledge',
    'Bookmarked for later reference',
]

video_ids = []
for i, (title, desc, ytid, views, likes, dislikes, cat) in enumerate(videos):
    vid = str(uuid.uuid4())
    video_ids.append(vid)
    uid = uids[i % 3]
    days = i * 7 + 1
    print(f'INSERT INTO "Videos" (id, title, description, upload_date, uploader_id, thumbnail_url, video_url, view_count, like_count, dislike_count, category) '
          f"VALUES ('{vid}', '{title}', '{desc}', NOW() - interval '{days} days', '{uid}', "
          f"'https://img.youtube.com/vi/{ytid}/hqdefault.jpg', 'https://www.youtube.com/watch?v={ytid}', {views}, {likes}, {dislikes}, '{cat}');")

for vid in video_ids:
    for j in range(3):
        cid = str(uuid.uuid4())
        ni = (abs(hash(vid)) + j) % 3
        ci = (abs(hash(vid)) + j) % len(comments_list)
        lc = random.randint(0, 200)
        hrs = random.randint(1, 720)
        print(f'INSERT INTO "Comments" (id, video_id, user_id, username, content, like_count, created_at) '
              f"VALUES ('{cid}', '{vid}', '{uids[ni]}', '{names[ni]}', '{comments_list[ci]}', {lc}, NOW() - interval '{hrs} hours');")
PYEOF

cat "$TMPFILE" | $CONTAINER_CMD exec -i "$PG_VIDEO" psql -U glense -d glense_video > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "  Inserted 8 videos with comments"
else
    echo "  ERROR: Failed to insert videos (is $PG_VIDEO running?)"
fi
rm -f "$TMPFILE"

echo ""
echo "=== Done! ==="
echo "All users have password: Password123!"
echo "Log in via the frontend at http://localhost:5173 (or next free port)"

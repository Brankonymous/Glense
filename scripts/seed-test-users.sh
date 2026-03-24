#!/usr/bin/env bash
# Seed test users and sample data for local development
# Usage: ./scripts/seed-test-users.sh [ACCOUNT_URL] [DONATION_URL]

ACCOUNT=${1:-http://localhost:5001}
DONATION=${2:-http://localhost:5100}

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
echo "=== Done! ==="
echo "All users have password: Password123!"
echo "Log in via the frontend at http://localhost:5173 (or next free port)"

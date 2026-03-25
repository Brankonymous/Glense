#!/bin/bash
# Seed test users for local development
# Usage: ./scripts/seed-test-users.sh [GATEWAY_URL]

GATEWAY=${1:-http://localhost:5050}
ACCOUNT=${2:-http://localhost:5001}

echo "Seeding test users via $ACCOUNT..."

register_user() {
    local username=$1 email=$2 type=$3
    local result
    result=$(curl -s -X POST "$ACCOUNT/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"email\": \"$email\",
            \"password\": \"Password123!\",
            \"confirmPassword\": \"Password123!\",
            \"accountType\": \"$type\"
        }")

    local user_id
    user_id=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin)['user']['id'])" 2>/dev/null)

    if [ -n "$user_id" ]; then
        echo "  Created $username ($type) -> $user_id"

        # Top up wallet with $500 for testing
        curl -s -X POST "$GATEWAY/api/wallet/user/$user_id/topup" \
            -H "Content-Type: application/json" \
            -d '{"amount": 500}' > /dev/null 2>&1
        echo "    Wallet topped up with \$500"
    else
        local msg
        msg=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin).get('message','unknown'))" 2>/dev/null)
        echo "  $username: $msg (may already exist)"
    fi
}

register_user "keki"    "keki@glense.test"    "creator"
register_user "irena"   "irena@glense.test"   "creator"
register_user "branko"  "branko@glense.test"   "user"

echo ""
echo "Done! All users have password: Password123!"
echo "You can log in via the frontend or test with curl."

#!/bin/bash

# Check if branch prefix argument is provided
if [ $# -eq 0 ]; then
    echo "Error: Branch prefix argument is required"
    echo "Usage: $0 <branch-prefix>"
    echo "Example: $0 brankonymous"
    exit 1
fi

# Configuration
BRANCH_PREFIX="$1"

# Setup git alias for pushing to specified branch prefix
git config alias.pp "!f() { bname=\$(git rev-parse --abbrev-ref HEAD); git push origin HEAD:${BRANCH_PREFIX}/\$bname; }; f"

echo "Git alias 'pp' has been configured successfully!"
echo "Usage: git pp"
echo "This will push your current branch to origin/${BRANCH_PREFIX}/<current-branch-name>"

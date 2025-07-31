#!/bin/bash

# Setup script for Git hooks
# Run this script to install the pre-commit hook for C# code formatting

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "${YELLOW}Setting up Git hooks...${NC}"

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(git rev-parse --show-toplevel)"

# Check if we're in a git repository
if [ ! -d "$REPO_ROOT/.git" ]; then
    echo "${RED}Error: Not in a git repository${NC}"
    exit 1
fi

# Create hooks directory if it doesn't exist
HOOKS_DIR="$REPO_ROOT/.git/hooks"
mkdir -p "$HOOKS_DIR"

# Create the pre-commit hook
cat > "$HOOKS_DIR/pre-commit" << 'EOF'
#!/bin/sh
#
# Pre-commit hook to format C# code
# This hook runs dotnet format on staged C# files before committing
#

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "${YELLOW}Running C# code formatting...${NC}"

# Get the root directory of the repository
REPO_ROOT=$(git rev-parse --show-toplevel)

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "${RED}Error: dotnet CLI is not installed or not in PATH${NC}"
    echo "Please install .NET SDK to use this pre-commit hook"
    exit 1
fi

# Get staged C# files
STAGED_CS_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep '\.cs$')

if [ -z "$STAGED_CS_FILES" ]; then
    echo "${GREEN}No C# files staged for commit. Skipping formatting.${NC}"
    exit 0
fi

echo "Found staged C# files:"
echo "$STAGED_CS_FILES"

# Check if there are any .csproj files in the repository
CS_PROJECTS=$(find "$REPO_ROOT" -name "*.csproj" -type f)

if [ -z "$CS_PROJECTS" ]; then
    echo "${YELLOW}Warning: No .csproj files found. Skipping formatting.${NC}"
    exit 0
fi

echo "Found .csproj files:"
echo "$CS_PROJECTS"

# Run dotnet format on the solution or projects
cd "$REPO_ROOT"

# Try to format using solution file first
if [ -f "Glense.sln" ]; then
    echo "Formatting using solution file: Glense.sln"
    dotnet format Glense.sln --verbosity quiet
    FORMAT_EXIT_CODE=$?
else
    echo "No solution file found, formatting individual projects"
    # Format each project file and collect exit codes
    FORMAT_EXIT_CODE=0
    for project in $CS_PROJECTS; do
        echo "Formatting project: $project"
        dotnet format "$project" --verbosity quiet
        PROJECT_EXIT_CODE=$?
        if [ $PROJECT_EXIT_CODE -ne 0 ]; then
            FORMAT_EXIT_CODE=$PROJECT_EXIT_CODE
        fi
    done
fi

if [ $FORMAT_EXIT_CODE -eq 0 ]; then
    echo "${GREEN}C# code formatting completed successfully!${NC}"
    
    # Check if any files were modified by formatting
    MODIFIED_FILES=$(git diff --name-only)
    if [ -n "$MODIFIED_FILES" ]; then
        echo "${YELLOW}The following files were modified by formatting:${NC}"
        echo "$MODIFIED_FILES"
        echo "${YELLOW}Please stage these changes and commit again.${NC}"
        exit 1
    fi
    
    exit 0
else
    echo "${RED}C# code formatting failed!${NC}"
    echo "Please fix any formatting issues and try again."
    exit 1
fi
EOF

# Make the hook executable
chmod +x "$HOOKS_DIR/pre-commit"

echo "${GREEN}Pre-commit hook installed successfully!${NC}"
echo "${YELLOW}The hook will now automatically format C# code before each commit.${NC}"
echo ""
echo "To test it, try making a change to a .cs file and committing:"
echo "  git add ."
echo "  git commit -m 'Test commit'" 

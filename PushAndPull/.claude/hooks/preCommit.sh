#!/bin/bash
# .claude/hooks/preCommit.sh
# Ensure tests pass before allowing dotnet build/run/publish (Claude Code PreToolUse hook)

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if [[ "$TOOL_NAME" != "Bash" ]]; then
    exit 0
fi

if [[ ! "$COMMAND" =~ dotnet[[:space:]]+(build|run|publish) ]]; then
    exit 0
fi

echo "[Hook] Checking tests before proceeding..."

dotnet test PushAndPull/PushAndPull.sln --nologo --no-build 2>/dev/null
RESULT=$?

if [ $RESULT -ne 0 ]; then
    echo "[Hook] ✗ Tests failed — fix tests before running dotnet $BASH_REMATCH[1]"
    exit 2
fi

echo "[Hook] ✓ Tests passed"
exit 0

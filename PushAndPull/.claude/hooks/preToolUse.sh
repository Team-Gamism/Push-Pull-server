#!/bin/bash
# .claude/hooks/preToolUser.sh
# Block dangerous commands before execution (Claude Code PreToolUse hook)

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

if [[ "$TOOL_NAME" != "Bash" ]]; then
    exit 0
fi

NORMALIZED=$(echo "$COMMAND" | tr -s ' ')

BLOCKED_PATTERNS=(
    "rm -rf /"
    "rm -rf \."
    "rm -rf ~"
    "rm -rf \*"
    "sudo .*rm"
    "> /dev/"
    "dd if="
    "mkfs"
    "curl .*\| .*sh"
    "curl .*\| .*bash"
    "wget .*\| .*sh"
    "wget .*\| .*bash"
)

for pattern in "${BLOCKED_PATTERNS[@]}"; do
    if [[ "$NORMALIZED" =~ $pattern ]]; then
        echo "[Hook] ✗ Blocked dangerous command"
        echo "Command: $COMMAND"
        echo "Reason: matched pattern '$pattern'"
        exit 2
    fi
done

exit 0

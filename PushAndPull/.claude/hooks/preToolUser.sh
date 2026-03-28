#!/bin/bash
# .claude/hooks/preToolUse.sh
# Block dangerous commands before execution

if [[ "$TOOL_NAME" == "Bash" ]]; then
    COMMAND="$TOOL_PARAMS_COMMAND"

    # normalize 
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
        "curl .*\\| .*sh"
        "curl .*\\| .*bash"
        "wget .*\\| .*sh"
        "wget .*\\| .*bash"
    )

    for pattern in "${BLOCKED_PATTERNS[@]}"; do
        if [[ "$NORMALIZED" =~ $pattern ]]; then
            echo "[Hook] ✗ Blocked dangerous command"
            echo "Command: $COMMAND"
            echo "Reason: matched pattern '$pattern'"
            exit 2
        fi
    done

    echo "[Hook] ✓ Command allowed"
fi
#!/bin/bash
# .claude/hooks/commit-msg.sh
# Validate commit message format for git commit commands (Claude Code PreToolUse hook)

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.tool_name // empty')
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only handle Bash tool
if [[ "$TOOL_NAME" != "Bash" ]]; then
    exit 0
fi

# Only handle git commit commands
if [[ ! "$COMMAND" =~ git[[:space:]]+commit ]]; then
    exit 0
fi

# Extract commit message from -m "..." or -m '...'
COMMIT_MSG=$(echo "$COMMAND" | grep -oP '(?<=-m )["\x27]?\K[^"'\'']+')

if [[ -z "$COMMIT_MSG" ]]; then
    exit 0
fi

# Trim whitespace
COMMIT_MSG=$(echo "$COMMIT_MSG" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')

TYPES="feat|fix|update|docs|refactor|test|chore"
PATTERN="^($TYPES): .+"

if [[ ! "$COMMIT_MSG" =~ $PATTERN ]]; then
    echo "[Hook] ✗ Invalid commit message format"
    echo ""
    echo "Expected: {type}: {Korean description}"
    echo "Allowed types: feat, fix, update, docs, refactor, test, chore"
    echo ""
    echo "Examples:"
    echo "  feat: 방 생성 API 추가"
    echo "  fix: 세션 DI 누락 수정"
    exit 2
fi

if [[ "$COMMIT_MSG" =~ [\.\!]$ ]]; then
    echo "[Hook] ✗ Do not end the message with punctuation"
    exit 2
fi

LENGTH=${#COMMIT_MSG}
if (( LENGTH < 10 )); then
    echo "[Hook] ✗ Commit message too short (min: 10 chars)"
    exit 2
fi

if (( LENGTH > 72 )); then
    echo "[Hook] ✗ Commit message too long (max: 72 chars)"
    exit 2
fi

echo "[Hook] ✓ Commit message valid"
exit 0

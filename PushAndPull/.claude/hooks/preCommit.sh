#!/bin/bash
# .claude/hooks/preCommit.sh

COMMIT_MSG="$TOOL_PARAMS_MESSAGE"

# allowed types
PATTERN="^(feat|fix|update|docs): .+"

if [[ ! "$COMMIT_MSG" =~ $PATTERN ]]; then
    echo "[Hook] ✗ Invalid commit message format"
    echo ""
    echo "Expected:"
    echo "  {type}: {Korean description}"
    echo ""
    echo "Types:"
    echo "  feat   — new feature"
    echo "  fix    — bug fix or missing DI/config"
    echo "  update — modification to existing code"
    echo "  docs   — documentation changes"
    echo ""
    echo "Examples:"
    echo "  feat: 방 생성 API 추가"
    echo "  fix: 세션 DI 누락 수정"
    echo "  update: Room 엔터티 수정"
    exit 1
fi

# punctuation check
if [[ "$COMMIT_MSG" =~ [\.\!]$ ]]; then
    echo "[Hook] ✗ Do not end the message with punctuation"
    echo "Example: feat: 방 생성 API 추가"
    exit 1
fi

# ensure single line
if [[ "$COMMIT_MSG" == *$'\n'* ]]; then
    echo "[Hook] ✗ Commit body is not allowed"
    echo "Use subject line only"
    exit 1
fi

echo "[Hook] ✓ Commit message format valid"

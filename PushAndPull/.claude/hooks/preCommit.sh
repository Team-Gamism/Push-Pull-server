#!/bin/bash
# .claude/hooks/preToolUse.sh
# Ensure tests pass before allowing dotnet-related operations

if [[ "$TOOL_NAME" == "Bash" ]]; then
    COMMAND="$TOOL_PARAMS_COMMAND"

    # Detect dotnet-related commands (build/run/commit-like flow)
    if [[ "$COMMAND" =~ dotnet\ (build|run|publish) ]]; then
        echo "[Hook] Checking tests before proceeding..."

        dotnet test --nologo --no-build
        RESULT=$?

        if [ $RESULT -ne 0 ]; then
            echo "[Hook] ✗ Tests failed"
            echo "Code changes must be reflected in tests before proceeding."
            exit 2
        fi

        echo "[Hook] ✓ Tests passed"
    fi
fi
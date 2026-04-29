#!/usr/bin/env bash
# Codex advisory review wrapper — always exits 0; never blocks the caller.
# Usage:
#   bash scripts/codex-review.sh --base origin/main [--timeout 120] [--raw]
#   bash scripts/codex-review.sh "Review for security issues" [--timeout 120] [--raw]
set -u

TIMEOUT=120
RAW=false
BASE=""
PROMPT=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --base)     BASE="$2";    shift 2 ;;
    --timeout)  TIMEOUT="$2"; shift 2 ;;
    --raw)      RAW=true;     shift   ;;
    *)          PROMPT="$1";  shift   ;;
  esac
done

TMPFILE=$(mktemp)
trap 'rm -f "$TMPFILE"' EXIT

# ------------------------------------------------------------------
# Output helpers
# ------------------------------------------------------------------
json_out() {  # status reason body
  local status="$1" reason="$2" body="$3"
  if command -v jq &>/dev/null; then
    jq -n --arg s "$status" --arg r "$reason" --arg b "$body" \
      '{status:$s,reason:$r,body:$b}'
  else
    local esc_body; esc_body=$(printf '%s' "$body" | sed 's/\\/\\\\/g; s/"/\\"/g; s/\t/\\t/g; s/$/\\n/' | tr -d '\n')
    printf '{"status":"%s","reason":"%s","body":"%s"}\n' "$status" "$reason" "$esc_body"
  fi
}

emit() {  # status reason body
  local status="$1" reason="$2" body="$3"
  if $RAW; then
    printf '[%s] %s\n' "$status" "$reason"
    [[ -n "$body" ]] && printf '%s\n' "$body"
  else
    json_out "$status" "$reason" "$body"
  fi
}

# ------------------------------------------------------------------
# Preflight checks
# ------------------------------------------------------------------
if ! command -v codex &>/dev/null; then
  emit "SKIPPED" "codex cli not found" ""
  exit 0
fi

if ! codex login status &>/dev/null 2>&1 && [[ -z "${OPENAI_API_KEY:-}" ]]; then
  emit "SKIPPED" "codex not authenticated" ""
  exit 0
fi

# ------------------------------------------------------------------
# Run Codex with a bash-native timeout (no gtimeout dependency)
# ------------------------------------------------------------------
run_with_timeout() {
  local cmd=("$@")
  "${cmd[@]}" >"$TMPFILE" 2>&1 &
  local pid=$!
  local elapsed=0
  while kill -0 "$pid" 2>/dev/null; do
    sleep 1
    (( elapsed++ ))
    if (( elapsed >= TIMEOUT )); then
      kill "$pid" 2>/dev/null
      wait "$pid" 2>/dev/null
      return 124  # timeout exit code
    fi
  done
  wait "$pid"
  return $?
}

# ------------------------------------------------------------------
# Choose mode: diff-based review vs free-form prompt
# ------------------------------------------------------------------
if [[ -n "$BASE" ]]; then
  # Check whether `codex review --base` subcommand exists
  if ! codex review --help 2>&1 | grep -q '\-\-base'; then
    emit "SKIPPED" "codex review --base not supported in this CLI version" ""
    exit 0
  fi
  run_with_timeout codex review --base "$BASE" \
    -c model_reasoning_effort=high \
    -c approval_policy=never
else
  run_with_timeout codex exec "$PROMPT" \
    -c model_reasoning_effort=high \
    -c approval_policy=never
fi

EXIT_CODE=$?
BODY=$(cat "$TMPFILE")

if [[ $EXIT_CODE -eq 124 ]]; then
  emit "TIMEOUT" "exceeded ${TIMEOUT}s" ""
  exit 0
fi

if [[ $EXIT_CODE -ne 0 ]]; then
  emit "ERROR" "codex exited with code ${EXIT_CODE}" "$BODY"
  exit 0
fi

emit "OK" "" "$BODY"
exit 0

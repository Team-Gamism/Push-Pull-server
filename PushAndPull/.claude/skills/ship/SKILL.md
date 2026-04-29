---
name: ship
argument-hint: <feature-description>
description: Ship a feature through a gated Claude implementation pipeline with non-blocking Codex advisory review.
allowed-tools: Bash, Read, Edit, Write, Agent
---

Ship the feature described in `$ARGUMENTS` through the following gated pipeline. Halt on any blocking failure.

---

## Phase 0 — Pre-check (blocking)

1. Verify there are no uncommitted changes: `git status --porcelain`
2. Report the current branch name.
3. If uncommitted changes exist, stop and ask the user to commit or stash them first.

---

## Phase 1 — Planning (user approval gate)

Spawn a sub-agent to produce a work contract for `$ARGUMENTS`:

- List every file to be created or modified (path + one-line reason)
- List every new endpoint or behavior change
- List edge cases and error paths to handle
- Estimate test scenarios needed

Present the contract to the user.

**Do NOT proceed to Phase 2 without explicit user approval.**

---

## Phase 2 — Implementation (blocking)

Implement the approved contract:

- Follow `.claude/rules/` conventions (architecture, code-style, domain-patterns, security, testing)
- Create or modify services, repositories, controllers, DTOs, and exceptions as needed
- Add or update unit tests in `PushAndPull.Test` mirroring production namespaces

---

## Phase 3 — Validation (blocking, auto-retry up to 2×)

```bash
dotnet build PushAndPull.sln
dotnet test PushAndPull.Test/PushAndPull.Test.csproj
```

If either command fails, diagnose and fix, then retry. After 2 failed retries, stop and report the failure details.

---

## Phase 3.5 — Codex Advisory Review (non-blocking)

```bash
bash scripts/codex-review.sh --base origin/main --raw --timeout 120
```

If the script is missing or Codex is unavailable, log `[SKIPPED]` and continue.
CRITICAL findings are recorded in the Phase 4 report but do **not** block the pipeline.

---

## Phase 4 — Report

Summarize results in this format:

```
## Ship Report: <feature-description>

| Phase | Status | Notes |
|-------|--------|-------|
| Pre-check    | ✅ / ❌ | ... |
| Planning     | ✅      | Approved by user |
| Implementation | ✅ / ❌ | files changed |
| Validation   | ✅ / ❌ | build + test results |
| Codex Advisory | ✅ SKIPPED / findings | see below |

### Codex Advisory Findings (advisory only — not blocking)
<findings or "none">
```

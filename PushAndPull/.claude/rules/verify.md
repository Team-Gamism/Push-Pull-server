---
description: Build-and-verify workflow. Run build then tests after any C# code change.
globs: ["**/*.cs"]
alwaysApply: false
---

## Build-and-Verify Workflow

After any `.cs` file change, the `postToolUse` hook runs build and tests automatically.

**If the build fails:** fix the build errors.

**If tests fail:**
- Production code bug → fix the production code.
- Test is outdated → fix the test code.

Only consider the task complete when the build succeeds and all tests pass.

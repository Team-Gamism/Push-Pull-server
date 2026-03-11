---
description: Run all tests and analyze failures
allowed-tools: Bash
---

Run the full test suite and report results.

## Steps

1. Run all tests:
   ```bash
   dotnet test PushAndPull.sln --logger "console;verbosity=normal"
   ```
2. Check the test output:
   - If all tests **pass**: confirm success and show the summary (total tests, duration)
   - If any tests **fail**: list each failing test by name, show the failure message and stack trace, then explain the likely cause and how to fix it

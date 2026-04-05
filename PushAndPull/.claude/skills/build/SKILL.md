---
name: build
description: Builds the PushAndPull project and reports errors. Use for checking build failures and debugging compile errors.
allowed-tools: Bash(dotnet build:*)
context: fork
---

Build the server project and report any errors.

## Steps

1. Run the build command:
   ```bash
   dotnet build PushAndPull/PushAndPull.csproj
   ```
2. Check the build output:
   - If the build **succeeds**: confirm success and show the summary (warnings, if any)
   - If the build **fails**: list each error with its file path and line number, then explain the likely cause and how to fix it

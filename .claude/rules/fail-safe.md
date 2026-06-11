---
paths:
  - "PushAndPull/**/*.cs"
  - "PushAndPull.Test/**/*.cs"
---

# Fail-Safe Rules

- Preserve existing public API behavior unless the task explicitly asks for a breaking change.
- Do not change authentication strategy from `Session-Id` header sessions without explicit approval.
- Do not bypass password hashing or compare raw private-room passwords directly outside the password hasher abstraction.
- Do not remove cancellation tokens from I/O paths.
- Do not broaden controller responsibilities when adding behavior; keep business logic in services.
- When unsure about a domain rule, add focused tests before changing behavior.

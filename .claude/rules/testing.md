---
paths:
  - "src/PushAndPull.Test/**/*.cs"
  - "src/PushAndPull/**/*.cs"
---

# Testing Rules

- Use xUnit and Moq for unit tests.
- Mirror production namespaces and folders under `PushAndPull.Test`.
- Structure scenario tests with nested classes named `When...`.
- Name test methods `It_...` and assert one behavior per test.
- Mock service dependencies through interfaces.
- Verify side effects with `Moq.Verify` and explicit `Times`.
- Add or update focused tests for service behavior, entity invariants, auth/session behavior, and repository query semantics when those areas change.

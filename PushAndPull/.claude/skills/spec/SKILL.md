---
argument-hint: [feature or topic to spec]
description: Interview user in-depth to create a production-ready spec
allowed-tools: AskUserQuestion, Read, Grep, Glob, Write
---

If `<instructions>` is empty, ask the user what feature or topic to spec before starting the interview.

The user can say "done" or "skip" at any point to stop the interview early and generate the spec with what has been gathered so far.

Before starting, scan the existing codebase (entities, services, controllers) to understand current structure — use this context to ask sharper, more relevant questions.

---

## Interview Rules

- Ask **non-obvious, high-signal questions only**
- Avoid basic or surface-level questions unless necessary
- Each question must uncover:
  - hidden constraints
  - edge cases
  - tradeoffs
  - failure scenarios
  - scalability concerns
- Ask **one question at a time**
- Adapt the next question based on the previous answer

## Required Coverage

Cover all of the following areas before finishing:

1. Core Problem & Goals
2. User Flows & UX edge cases
3. Data Model & State transitions
4. API design (request/response, validation, error cases)
5. Concurrency & consistency issues
6. Performance & scaling considerations
7. Failure handling & recovery
8. Security & abuse scenarios
9. Observability (logging, metrics, debugging)
10. Deployment & environment assumptions

Do NOT stop interviewing until all 10 areas are clearly defined and no major ambiguity remains.

---

## Output

Write a **production-ready spec** to `docs/spec/{feature-name}.md` including:

- Overview
- Goals & Non-goals
- Architecture
- Data model
- API contract
- Flow diagrams (text-based)
- Edge cases
- Failure scenarios
- Tradeoffs
- Open questions

---
description: Create Git commits by splitting changes into logical units
allowed-tools: Bash
---

Create Git commits following the project's commit conventions.

## Commit Message Format

```
{type}: {Korean description}
```

**Types**:
- `feat` — new feature added
- `fix` — bug fix, missing config, or missing DI registration
- `update` — modification to existing code

**Description rules**:
- Written in **Korean**
- Short and imperative (단문)
- No trailing punctuation (`.`, `!` etc.)
- Avoid noun-ending style — prefer verb style

**Examples**:
```
feat: 방 생성 API 추가
fix: 세션 DI 누락 수정
update: Room 엔터티 수정
```

**Do NOT**:
- Add Claude as co-author
- Write descriptions in English
- Add a commit body — subject line only

## Splitting Rules

**Changes must be split into logical units. Never combine unrelated changes into a single commit.**

Splitting criteria:
- Different domains → separate commits (Auth changes / Room changes)
- Different roles → separate commits (feature addition / bug fix / refactoring)
- Multiple files with a single purpose → can be grouped into one commit

Example — correct splitting:
```
feat: 방 참여 API 추가          ← Room feature files
fix: 세션 DI 누락 수정          ← Auth DI registration fix
update: Room 엔터티 수정        ← Room Entity change
```

Example — incorrect splitting:
```
feat: 방 참여 API 추가 및 세션 버그 수정   ← two purposes in one commit ❌
```

## Steps

1. Run `git status` and `git diff` to review all changes
2. Classify changed files into logical groups:
   - New feature → `feat`
   - Bug fix / missing registration → `fix`
   - Modification to existing code → `update`
3. If there are 2+ groups, they must be committed separately
4. For each group:
   - Stage only the relevant files with `git add <files>`
   - Write a commit message following the format above
   - Run `git commit -m "message"`
5. Verify with `git log --oneline -n {commit count}`

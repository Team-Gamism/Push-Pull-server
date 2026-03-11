---
description: Generate PR title suggestions and body based on changes from develop
allowed-tools: Bash(git log:*), Bash(git diff:*), Bash(git branch:*), Bash(gh pr create:*), Write, AskUserQuestion
---

Generate a PR title and body for the current branch based on changes from `develop`.

## Context

- Current branch: !`git branch --show-current`
- Commits from develop: !`git log develop..HEAD --oneline`
- File change stats from develop: !`git diff develop...HEAD --stat`
- Detailed diff from develop: !`git diff develop...HEAD`

## PR Title Convention

Format: `{type}: {Korean description}`

**Types:**
- `feature` — new feature added
- `fix` — bug fix or missing configuration/DI registration
- `modify` — modification to existing code
- `refactor` — refactoring without behavior change
- `release` — release (use format `release/x.x.x`)

**Rules:**
- Description in Korean
- Short and imperative (단문)
- No trailing punctuation

**Examples:**
- `feature: 방 생성 API 추가`
- `fix: Key Vault 연동 방식을 AddAzureKeyVault으로 변경`
- `refactor: 로그인 로직 리팩토링`

## PR Body Template

Follow this exact structure (keep the emoji headers as-is):

```
## 📚작업 내용

- {change item 1}
- {change item 2}

## ◀️참고 사항

{additional notes, context, before/after comparisons if relevant. Write "." if nothing to add.}

## ✅체크리스트

> `[ ]`안에 x를 작성하면 체크박스를 체크할 수 있습니다.

- [x] 현재 의도하고자 하는 기능이 정상적으로 작동하나요?
- [x] 변경한 기능이 다른 기능을 깨뜨리지 않나요?


> *추후 필요한 체크리스트는 업데이트 될 예정입니다.*
```

## Your Task

1. **Suggest 3 PR titles** following the convention above.

2. **Write the PR body**:
   - Analyze commits and diffs from develop
   - Fill in `작업 내용` with a concise bullet list of what changed
   - Fill in `참고 사항` with any important context (architecture decisions, before/after, caveats). Write `.` if nothing relevant.
   - Keep total body under 2500 characters
   - Write in Korean
   - No emojis in text content (keep the section header emojis)

3. **Save to file** using the Write tool:
   - Path: `PR_BODY.md`
   - Overwrite if it already exists

4. **Output** in this format:
   ```
   ## 추천 PR 제목

   1. [title1]
   2. [title2]
   3. [title3]

   ## PR 본문 (PR_BODY.md에 저장됨)

   [full body preview]
   ```

5. **Ask the user** using the AskUserQuestion tool:
   - "어떤 제목을 사용할까요? (1 / 2 / 3 또는 직접 입력)"

6. **Create the PR** using the chosen title:
   - If the user answered 1, 2, or 3, use the corresponding suggested title
   - If the user typed a custom title, use it as-is
   - Run:
     ```bash
     gh pr create --title "{chosen title}" --body-file PR_BODY.md --base develop
     ```
   - Output the PR URL when done

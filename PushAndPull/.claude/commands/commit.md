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
- `modify` — modification to existing code

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

**변경사항은 반드시 논리적 단위로 분리하여 커밋한다. 관련 없는 변경을 하나의 커밋에 합치지 않는다.**

분리 기준:
- 도메인이 다르면 별도 커밋 (Auth 변경 / Room 변경)
- 역할이 다르면 별도 커밋 (기능 추가 / 버그 수정 / 리팩터링)
- 파일이 여러 개라도 하나의 목적이면 같은 커밋으로 묶을 수 있음

예시 — 올바른 분리:
```
feat: 방 참여 API 추가          ← Room 기능 추가 파일들
fix: 세션 DI 누락 수정          ← Auth DI 등록 누락 수정
modify: Room 엔터티 수정        ← Room Entity 변경
```

예시 — 잘못된 분리:
```
feat: 방 참여 API 추가 및 세션 버그 수정   ← 두 가지 목적을 하나로 묶음 ❌
```

## Steps

1. `git status` 와 `git diff` 로 전체 변경사항 확인
2. 변경된 파일을 논리적 단위로 그룹 분류:
   - 새 기능 추가 → `feat`
   - 버그 수정 / 누락된 등록 수정 → `fix`
   - 기존 코드 수정 → `modify`
3. 그룹이 2개 이상이면 반드시 별도 커밋으로 나눈다
4. 각 그룹별로:
   - `git add <files>` 로 해당 파일만 스테이징
   - 위 형식에 맞게 커밋 메시지 작성
   - `git commit -m "message"` 실행
5. `git log --oneline -n {커밋 수}` 로 결과 확인

# [Refactor] 오래된 worktree 사본 정리

## 우선순위
🟢 여유 있을 때 (정리 작업)

## 현황
`PushAndPull/.claude/worktrees/trusting-beaver-7e4a30/`에
저장소 전체 사본이 남아 있다. gitignore에는 등록돼 있어 커밋되지는 않지만,
검색·빌드 시 노이즈가 되고 디스크를 차지한다.

## 작업
```bash
git worktree remove .claude/worktrees/trusting-beaver-7e4a30
git branch -D claude/trusting-beaver-7e4a30  # 병합 여부 확인 후
```

## 완료 조건
- [ ] worktree 디렉터리 제거
- [ ] `git worktree list`에 잔여 항목 없음

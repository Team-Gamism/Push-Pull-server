# Sub-Agent Workflow Guide

## Available Agents

| Agent | Role | When to Use |
|---|---|---|
| `researcher` | Codebase analysis & doc gathering | Before implementation — understand existing code, patterns, dependencies |
| `planner` | Implementation plan creation | After research — create step-by-step blueprint for changes |
| `reviewer` | Code review & validation | After implementation — catch bugs, security issues, convention violations |
| `web-researcher` | Live web research | When current external info is needed (releases, CVEs, library docs) |

## Workflow Patterns

### Full Pipeline: Research -> Plan -> Implement -> Review

For complex features or multi-file changes:

1. **Researcher** gathers context about affected code, patterns, and constraints
2. **Planner** creates a step-by-step implementation plan from the research
3. **Developer/Main agent** implements the plan
4. **Reviewer** validates the implementation

### Quick Pipeline: Plan -> Implement -> Review

For well-understood changes where context is already clear:

1. **Planner** creates the implementation plan
2. **Developer/Main agent** implements
3. **Reviewer** validates

### Parallel Research

For broad investigations, run multiple agents simultaneously:

- **Researcher** analyzes the codebase
- **Web-Researcher** fetches external documentation
- Results feed into the **Planner**

## Invocation

Each agent can be triggered via the `Agent` tool with `subagent_type` matching the agent name, or by natural language trigger phrases:

- `researcher 실행해` / `코드 조사해줘`
- `planner 실행해` / `구현 계획 세워줘`
- `reviewer 실행해` / `코드 리뷰해줘`
- `web-researcher 실행해` / `최신 정보 조사해줘`

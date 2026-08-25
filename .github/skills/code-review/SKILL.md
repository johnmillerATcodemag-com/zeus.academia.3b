---
ai_generated: true
model: "openai/gpt-5.4@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-08-25-code-review-agent-guardrails"
prompt: |
  Create a repository-level code-review skill that catches startup wiring drift and duplicated validation logic before merge.
started: "2026-08-25T10:00:00Z"
ended: "2026-08-25T10:15:00Z"
task_durations:
  - task: "review guardrail gaps"
    duration: "00:05:00"
  - task: "draft skill content"
    duration: "00:07:00"
  - task: "validate repository fit"
    duration: "00:03:00"
total_duration: "00:15:00"
ai_log: "ai-logs/2026/08/25/2026-08-25-code-review-agent-guardrails/conversation.md"
source: ".github/instructions/custom-agents.instructions.md"
---

# Code Review Guardrails

Use this skill when reviewing a change that introduces routes, handlers, validators, or any value normalization logic.

## Required review checks

1. Startup wiring
   - Confirm every new `Map...Endpoints()` or route aggregator is registered in the app host or composition root.
   - If a route is not reachable at runtime, flag it as a blocking issue.

2. Single source of truth
   - Confirm validation, coercion, normalization, and numeric-range rules are not duplicated across handlers, validators, and mapping helpers.
   - Prefer an existing shared helper or domain primitive over re-implementing the same logic.

3. Runtime reachability
   - Treat compile-only success as insufficient evidence for route-based work.
   - Require a startup or integration verification step for endpoint changes.

4. Drift prevention
   - Look for neighboring slices or prior implementations that already solved the same rule and reuse that pattern.
   - Flag drift when the same invariant is implemented in multiple places with slightly different logic.

## Output format

Return:

- issue summary
- file(s) involved
- root cause
- fix recommendation
- severity: blocking or advisory

## Blocking examples

- A new endpoint file exists but is never called from app startup.
- A validator and a handler both implement the same numeric normalization rule with different boundaries.
- A route appears to compile but has no startup registration or runtime verification evidence.

## Review policy

- Prefer high-confidence findings.
- Keep the review focused on correctness, runtime wiring, and rule reuse.
- Do not mark a feature as complete without confirming registration and rule reuse.

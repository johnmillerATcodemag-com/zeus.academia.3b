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
   - Require evidence that the route was registered from the host and not just defined in a feature file.
   - A route is not complete when the host compiles but the mapper is never called.

2. Validation contract correctness
   - If an endpoint declares `.ProducesValidationProblem()` or a validation response contract, confirm the actual exception-to-response mapping exists for argument and normalization failures.
   - A 500 raised by an unhandled `ArgumentException`, `ArgumentOutOfRangeException`, or equivalent value-normalization issue is a blocking issue when the contract promises validation output.
   - Require direct tests for invalid numbers, ranges, and malformed inputs when validation behavior is part of the slice.
   - Confirm validators are actually registered in DI or the MediatR validation pipeline for the request types that advertise validation responses.

3. Single source of truth
   - Confirm validation, coercion, normalization, and numeric-range rules are not duplicated across handlers, validators, and mapping helpers.
   - Prefer an existing shared helper or domain primitive over re-implementing the same logic.
   - For `Try*` APIs, reject implementations that return a non-null placeholder when the boolean result is `false`.
   - Flag unreachable catch blocks and dead exception handling that suggest a mismatched failure model or impossible exception path.

4. Configuration and migration ownership
   - Verify the same runtime dependency does not split across incompatible configuration sources (for example `ZEUS_SQLSERVER_CONNECTION` vs `ConnectionStrings:DefaultConnection`).
   - Confirm feature-local DbContexts that participate in `Database.MigrateAsync()` declare migration ownership and host invocation explicitly.
   - If the feature changes schema, ensure migration artifacts and ownership are present before approval.

5. Runtime reachability and contract parity
   - Treat compile-only success as insufficient evidence for route-based work.
   - Require a startup or integration verification step for endpoint changes.
   - If `.ProducesValidationProblem()`, `.Produces(409)`, or similar status codes are declared, confirm the route actually returns that result instead of leaking a raw exception or 500.

6. Drift prevention
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
- A host uses `ZEUS_SQLSERVER_CONNECTION` while a feature service registration silently reads `ConnectionStrings:DefaultConnection` and the configuration source is split.
- A feature-local DbContext runs `Database.MigrateAsync()` without explicit migration ownership or startup wiring evidence.
- A feature-local DbContext changes schema without migration artifacts.
- A validator and a handler both implement the same numeric normalization rule with different boundaries.
- A `Try*` method returns a placeholder object even when the bool result is `false`.
- A route advertises validation or conflict responses but throws a raw exception or 500 instead.
- A route appears to compile but has no startup registration or runtime verification evidence.
- An endpoint advertises validation-problem responses but unhandled argument/normalization exceptions still bubble as 500s.
- A validator or numeric-rule helper is introduced without direct invalid-input tests and validation contract coverage.
- Unreachable catch blocks remain even though the actual guard path is already enforced earlier in the code.

## Review policy

- Prefer high-confidence findings.
- Keep the review focused on correctness, runtime wiring, validation registration, schema ownership, and rule reuse.
- Do not mark a feature as complete without confirming registration, migration ownership, validation behavior, and rule reuse.

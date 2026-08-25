---
ai_generated: true
model: "openai/gpt-5.4@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "616990b5-0c5d-4735-a876-23fd1ebb4ff6"
prompt: |
  Create an implementation prompt for each slice in the #file:academia-execution-plan.md
started: "2026-04-20T20:40:00Z"
ended: "2026-04-20T21:40:00Z"
task_durations:
  - task: "analyze slice dependencies"
    duration: "00:15:00"
  - task: "draft slice implementation prompts"
    duration: "00:35:00"
  - task: "traceability and indexing"
    duration: "00:10:00"
total_duration: "01:00:00"
ai_log: "ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md"
source: ".github/models/workflows/academia-execution-plan.md"
---

# Academia Implementation Prompts

Implementation prompt set derived from `.github/models/workflows/academia-execution-plan.md`.

## Phase 0

- `ep-0-1-shared-kernel-implementation.prompt.md`
- `application-host-and-persistence-composition-implementation.prompt.md`

## Phase 1

- `ep-1-1-manage-ranks-implementation.prompt.md`
- `ep-1-2-manage-degrees-implementation.prompt.md`
- `ep-1-3-manage-universities-implementation.prompt.md`
- `ep-1-4-provision-extension-implementation.prompt.md`

Execution maps:

- [EP-1-3 ManageUniversities execution map](ep-1-3-manage-universities-execution-map.md) - independent artifact ownership, route/schema decisions, blockers, and verification scope.
- [EP-1-4 ProvisionExtension execution map](ep-1-4-provision-extension-execution-map.md) - independent artifact ownership, route/schema decisions, blockers, and verification scope.

## Phase 2

- `ep-2-1-register-academic-implementation.prompt.md`

## Phase 3

- `ep-3-1-view-academic-profile-implementation.prompt.md`
- `ep-3-2-update-academic-name-implementation.prompt.md`
- `ep-3-3-search-list-academics-implementation.prompt.md`
- `ep-3-4-grant-tenure-implementation.prompt.md`
- `ep-3-5-assign-contract-implementation.prompt.md`
- `ep-3-6-remove-employment-status-implementation.prompt.md`
- `ep-3-7-change-rank-implementation.prompt.md`
- `ep-3-8-record-degree-obtained-implementation.prompt.md`
- `ep-3-9-assign-extension-implementation.prompt.md`

## Phase 4

- `ep-4-1-renew-contract-implementation.prompt.md`
- `ep-4-2-convert-contract-to-tenure-implementation.prompt.md`
- `ep-4-3-update-degree-university-implementation.prompt.md`
- `ep-4-4-remove-degree-record-implementation.prompt.md`
- `ep-4-5-list-qualifications-implementation.prompt.md`
- `ep-4-6-reassign-extension-implementation.prompt.md`
- `ep-4-7-release-extension-implementation.prompt.md`
- `ep-4-8-list-available-extensions-implementation.prompt.md`

## Phase 5

- `ep-5-1-deregister-academic-implementation.prompt.md`

## Phase 6

- `ep-6-1-academic-directory-implementation.prompt.md`
- `ep-6-2-by-rank-report-implementation.prompt.md`
- `ep-6-3-by-access-level-report-implementation.prompt.md`
- `ep-6-4-tenured-academics-report-implementation.prompt.md`
- `ep-6-5-contracted-academics-report-implementation.prompt.md`
- `ep-6-6-expiring-contracts-report-implementation.prompt.md`
- `ep-6-7-qualification-reports-implementation.prompt.md`
- `ep-6-8-access-level-distribution-report-implementation.prompt.md`

## Notes

- Every prompt is slice-scoped and follows `.github/instructions/implementation-prompt.instructions.md`.
- The prompts assume the dependency order in `.github/models/workflows/academia-execution-plan.md` remains authoritative.
- If the codebase folder layout differs from the plan, update the prompt targets before implementation rather than forcing the code into a mismatched structure.

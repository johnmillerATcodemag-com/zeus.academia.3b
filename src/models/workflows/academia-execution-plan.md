---
ai_generated: true
model: "openai/gpt-5.4@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "616990b5-0c5d-4735-a876-23fd1ebb4ff6"
prompt: |
  Follow instructions in .github/prompts/create-academia-execution-plan.prompt.md
  using:
    project_overview_file: .github/instructions/project-overview.instructions.md
    implementation_plan_file: .github/models/workflows/academia-implementation-plan.md
    output_file: .github/models/workflows/academia-execution-plan.md
started: "2026-04-20T19:55:00Z"
ended: "2026-04-20T20:12:00Z"
task_durations:
  - task: "read inputs and dependency rules"
    duration: "00:04:00"
  - task: "draft phased execution plan"
    duration: "00:08:00"
  - task: "map slices to backlog and quality gates"
    duration: "00:04:00"
  - task: "write artifact and repository traceability updates"
    duration: "00:01:00"
total_duration: "00:17:00"
ai_log: "ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md"
source: ".github/prompts/create-academia-execution-plan.prompt.md"
---

# Zeus Academia Execution Plan

## Scope and Inputs

This plan converts the project constraints and vertical-slice dependency model into an execution order that can be implemented without re-deriving prerequisites.

Inputs used:

- `.github/instructions/project-overview.instructions.md`
- `.github/models/workflows/academia-implementation-plan.md`
- `.github/prompts/create-academia-execution-plan.prompt.md`

Constraint summary:

- CQRS with MediatR, FluentValidation, and vertical-slice folders under `src/features/`
- .NET 8+ backend and Vue 3 frontend with strict typing and 80% coverage target
- OAuth 2.0/OIDC, RBAC, HTTPS-only, FERPA/GDPR expectations
- API target under 500ms p95 and initial page load under 3 seconds
- Reporting slices must use read-optimized queries after their data-producing predecessors are complete

## Planning Assumptions

- Shared Kernel is a hard prerequisite for every slice and ships before any endpoint work.
- Phase 1 contains every independent reference-data slice that can be built in parallel.
- `RegisterAcademic` is the first sequential gate and must complete before any dependent slice starts.
- Report slices stay after the command/query slices that create or mutate their source data.
- Database constraints back up handler guards for `empNr`, extension assignment uniqueness, and qualification uniqueness.
- Each phase ends with passing automated tests, validator coverage for business rules, and API-level verification where endpoints exist.
- No new slices are introduced beyond the implementation plan.

## Dependency-Driven Phase Plan

### Phase 0

**Objective:** establish the Shared Kernel and persistence foundations used by every later slice.

**Included slices:** Shared Kernel; Application Host and Persistence Composition

**Blockers/Dependencies:** none

**Deliverables:**

- `Academic` aggregate with employment-state guards
- `Rank`, `AccessLevel`, `Degree`, `University`, and `Extension` value objects
- `AcademicQualification`, `Result<T>`, `Error`, domain event interfaces, and common exceptions
- reusable EF Core mapping semantics and invariant backing rules
- application host, dependency-injection composition, endpoint registration, SQL Server configuration, and migration execution policy
- explicit feature-local DbContext and migration ownership for each persisted table

**Acceptance criteria:**

- [ ] ExclusiveOr employment rule enforced in the aggregate
- [ ] AccessLevel derived from Rank only
- [ ] Shared Kernel types compile with nullable reference types enabled
- [ ] Foundational unit tests pass for guards, derivation, and result handling

**Test strategy:** unit tests for aggregate invariants, mapping tests for persistence, and migration validation for required constraints.

### Phase 1

**Objective:** build the independent reference-data and provisioning slices that unblock academic registration.

**Included slices:** `ManageRanks`, `ManageDegrees`, `ManageUniversities`, `ProvisionExtension`

**Blockers/Dependencies:** Phase 0, including Application Host and Persistence Composition

**Deliverables:**

- CRUD-lite commands/queries for rank, degree, and university reference data
- extension provisioning and deprovisioning with assignment guard
- seed or admin-path setup for baseline reference data

**Acceptance criteria:**

- [ ] Rank, degree, and university codes are unique and queryable
- [ ] Only valid rank codes participate in access-level mapping
- [ ] Assigned extensions cannot be deprovisioned
- [ ] Reference-data APIs or seed workflows are verified end to end

**Test strategy:** command/validator unit tests, repository tests, and integration tests for uniqueness and deprovisioning guards.

### Phase 2

**Objective:** deliver the first mandatory sequential gate by enabling academic registration.

**Included slices:** `RegisterAcademic`

**Blockers/Dependencies:** Phase 1

**Deliverables:**

- register-academic command, validator, handler, endpoint, and persistence workflow
- creation flow that attaches at least one qualification and one unassigned extension

**Acceptance criteria:**

- [ ] `empNr` is unique and fixed to 6 characters
- [ ] `EmpName` is capped at 15 characters
- [ ] registration requires at least one degree and university pair
- [ ] assigned or unavailable extensions are rejected
- [ ] successful registration persists rank-derived access level and initial qualifications

**Test strategy:** integration-first tests covering successful registration, duplicate `empNr`, invalid rank, missing qualification, and extension conflicts.

### Phase 3

**Objective:** build the first wave of slices that depend directly on registered academics and can proceed in parallel.

**Included slices:** `ViewAcademicProfile`, `UpdateAcademicName`, `SearchListAcademics`, `GrantTenure`, `AssignContract`, `RemoveEmploymentStatus`, `ChangeRank`, `RecordDegreeObtained`, `AssignExtension`

**Blockers/Dependencies:** Phase 2, plus Phase 1 reference data where required

**Deliverables:**

- profile and search queries
- employment-state commands and rank-change command
- qualification-add command
- extension assignment flow

**Acceptance criteria:**

- [ ] profile and search queries surface derived access level and current employment status
- [ ] tenure and contract flows preserve the ExclusiveOr rule
- [ ] contract dates are future-dated where required
- [ ] rank changes immediately recalculate access level
- [ ] extension assignment remains 1:1 between academic and extension
- [ ] duplicate academic-degree pairs are rejected when recording qualifications

**Test strategy:** API integration tests for read models and filters, plus command tests for employment, rank, qualification, and extension rules.

### Phase 4

**Objective:** complete the slices that depend on Phase 3 state transitions.

**Included slices:** `RenewContract`, `ConvertContractToTenure`, `UpdateDegreeUniversity`, `RemoveDegreeRecord`, `ListQualifications`, `ReassignExtension`, `ReleaseExtension`, `ListAvailableExtensions`

**Blockers/Dependencies:** Phase 3

**Deliverables:**

- renewal and contract-to-tenure flows
- qualification maintenance and qualification listing queries
- extension reassignment, release, and availability queries

**Acceptance criteria:**

- [ ] renewals only apply to currently contracted academics and require future dates
- [ ] conversion to tenure clears contract end date and preserves XOR employment logic
- [ ] degree-university updates only target existing qualification records
- [ ] removing a degree never leaves an academic with zero qualifications
- [ ] released extensions return to the available pool and reassignments preserve uniqueness

**Test strategy:** focused integration tests for stateful transitions, plus query tests for qualification and extension availability results.

### Phase 5

**Objective:** close the academic lifecycle and make data stable for reporting.

**Included slices:** `DeregisterAcademic`

**Blockers/Dependencies:** Phase 4 and prior registration flow

**Deliverables:**

- deregistration command, endpoint, event publication, and extension-release behavior

**Acceptance criteria:**

- [ ] deregistration requires the academic to exist
- [ ] assigned extensions are released as part of deregistration flow
- [ ] qualification history is retained according to domain rules
- [ ] deregistration emits the required domain event for downstream consumers

**Test strategy:** integration tests for deregistration, released extension state, and retention of qualification data.

### Phase 6

**Objective:** implement all report slices after their source data and events are stable.

**Included slices:** `AcademicDirectory`, `ByRankReport`, `ByAccessLevelReport`, `TenuredAcademicsReport`, `ContractedAcademicsReport`, `ExpiringContractsReport`, `QualificationReports`, `AccessLevelDistributionReport`

**Blockers/Dependencies:** Phases 3 through 5 according to each report's source data

**Deliverables:**

- read-optimized report queries and projection storage where needed
- filtering, sorting, pagination, and grouped aggregates required by workflows 7.1 to 7.9

**Acceptance criteria:**

- [ ] each report is implemented only after its source commands are complete
- [ ] contract reports honor active and expiring date filters
- [ ] qualification reports reflect grouped degree and university views
- [ ] rank and access-level reports align with derived access-level rules
- [ ] report queries meet performance expectations on seeded data volumes

**Test strategy:** projection and query integration tests, date-window tests for expiring contracts, and performance checks on representative datasets.

## Phase Backlog

- **ID**: `EP-0-1`
- **Slice**: `Shared Kernel`
- **Type**: `Shared`
- **Why now**: Every slice depends on the aggregate, value objects, and common persistence rules.
- **Implementation tasks**:
  - Create aggregate and value-object types used across the domain.
  - Encode employment guards and rank-to-access-level derivation.
  - Add domain events, result wrappers, and common exceptions.
  - Configure EF Core mappings and foundational constraints.
- **Definition of done**:
  - [ ] Aggregate invariants are enforced in code and covered by tests.
  - [ ] Database constraints exist for core uniqueness rules.
  - [ ] Shared Kernel builds cleanly and is reusable by all slices.

- **ID**: `EP-1-1`
- **Slice**: `ManageRanks`
- **Type**: `Command+Query`
- **Why now**: `RegisterAcademic` and `ChangeRank` both require valid rank data first.
- **Implementation tasks**:
  - Add add/list rank handlers and validators.
  - Restrict codes to `P`, `SL`, and `L`.
  - Expose rank data to downstream registration flow.
- **Definition of done**:
  - [ ] Invalid rank codes are rejected.
  - [ ] Rank records are queryable and unique.
  - [ ] Access-level mapping is verified against rank values.

- **ID**: `EP-1-2`
- **Slice**: `ManageDegrees`
- **Type**: `Command+Query`
- **Why now**: academic registration and qualification recording require canonical degree data.
- **Implementation tasks**:
  - Add add/list degree handlers and validators.
  - Enforce unique degree codes.
  - Seed or expose baseline degree data.
- **Definition of done**:
  - [ ] Duplicate degree codes are rejected.
  - [ ] Degree queries return stable reference data.
  - [ ] Registration can resolve requested degree codes.

- **ID**: `EP-1-3`
- **Slice**: `ManageUniversities`
- **Type**: `Command+Query`
- **Why now**: academic registration and qualification updates require canonical university data.
- **Implementation tasks**:
  - Add add/list university handlers and validators.
  - Enforce unique university codes.
  - Seed or expose baseline university data.
- **Definition of done**:
  - [ ] Duplicate university codes are rejected.
  - [ ] University queries return stable reference data.
  - [ ] Registration can resolve requested university codes.

- **ID**: `EP-1-4`
- **Slice**: `ProvisionExtension`
- **Type**: `Command`
- **Why now**: registration and later extension workflows require an available extension pool.
- **Implementation tasks**:
  - Add provision and deprovision commands.
  - Validate numeric extension format.
  - Block deprovisioning when an extension is assigned.
- **Definition of done**:
  - [ ] Provisioned extensions can be retrieved for assignment.
  - [ ] Assigned extensions cannot be deprovisioned.
  - [ ] Extension records remain unique and traceable.

- **ID**: `EP-2-1`
- **Slice**: `RegisterAcademic`
- **Type**: `Command`
- **Why now**: it is the hard prerequisite for every dependent academic slice.
- **Implementation tasks**:
  - Add command, validator, handler, and endpoint.
  - Validate `empNr`, name length, rank, qualifications, and extension availability.
  - Persist academic, qualifications, and extension linkage atomically.
- **Definition of done**:
  - [ ] Registration succeeds only with valid reference data and an unassigned extension.
  - [ ] Duplicate `empNr` and invalid payloads fail deterministically.
  - [ ] Created academic can be retrieved immediately by dependent queries.

- **ID**: `EP-3-1`
- **Slice**: `ViewAcademicProfile`
- **Type**: `Query`
- **Why now**: it depends only on registered academic data and supports immediate user visibility.
- **Implementation tasks**:
  - Build profile DTO with rank, access level, extension, qualifications, and employment state.
  - Add query handler and endpoint.
  - Cover not-found and happy-path responses.
- **Definition of done**:
  - [ ] Returned profile includes derived access level.
  - [ ] Qualifications and extension state are included.
  - [ ] Query behavior is covered by integration tests.

- **ID**: `EP-3-2`
- **Slice**: `UpdateAcademicName`
- **Type**: `Command`
- **Why now**: it is an isolated mutation that depends only on registration existing.
- **Implementation tasks**:
  - Add rename command and validator.
  - Enforce name length constraint.
  - Persist change and verify query visibility.
- **Definition of done**:
  - [ ] Names longer than 15 characters are rejected.
  - [ ] Updated name is visible through profile and list queries.
  - [ ] Command path is integration tested.

- **ID**: `EP-3-3`
- **Slice**: `SearchListAcademics`
- **Type**: `Query`
- **Why now**: it becomes useful immediately after registration exists and before reports are built.
- **Implementation tasks**:
  - Add filtered and paginated query.
  - Support filters for name, rank, access level, employment status, degree, and university.
  - Verify stable sort and pagination behavior.
- **Definition of done**:
  - [ ] Filters return expected subsets.
  - [ ] Pagination and sorting are deterministic.
  - [ ] Query performance is acceptable on seeded data.

- **ID**: `EP-3-4`
- **Slice**: `GrantTenure`
- **Type**: `Command`
- **Why now**: tenure state can be applied once an academic exists.
- **Implementation tasks**:
  - Add tenure command and handler.
  - Reuse employment guard methods in the aggregate.
  - Publish employment change events if required.
- **Definition of done**:
  - [ ] Tenure cannot coexist with contract end date.
  - [ ] Successful tenure updates profile/query outputs.
  - [ ] Rule coverage exists at unit and integration level.

- **ID**: `EP-3-5`
- **Slice**: `AssignContract`
- **Type**: `Command`
- **Why now**: contract assignment is another direct employment mutation after registration.
- **Implementation tasks**:
  - Add contract command, validator, and handler.
  - Enforce future-dated contract end dates.
  - Reuse XOR employment guards.
- **Definition of done**:
  - [ ] Past or current contract dates are rejected.
  - [ ] Contracted academics are excluded from tenure state simultaneously.
  - [ ] Result is visible in profile and report seed data.

- **ID**: `EP-3-6`
- **Slice**: `RemoveEmploymentStatus`
- **Type**: `Command`
- **Why now**: it depends only on an existing academic and existing employment state rules.
- **Implementation tasks**:
  - Add clear-employment command and handler.
  - Verify both tenure and contract state are reset cleanly.
  - Cover transition edge cases.
- **Definition of done**:
  - [ ] Employment state can be cleared from either prior path.
  - [ ] Aggregate remains valid after reset.
  - [ ] Query outputs reflect cleared employment state.

- **ID**: `EP-3-7`
- **Slice**: `ChangeRank`
- **Type**: `Command`
- **Why now**: it depends on registration and valid rank data, but no later sequential slice.
- **Implementation tasks**:
  - Add change-rank command and validator.
  - Recalculate access level automatically in the aggregate.
  - Publish rank-change event for downstream reports.
- **Definition of done**:
  - [ ] Only valid rank codes are accepted.
  - [ ] Access level changes immediately with rank.
  - [ ] Report-producing events are emitted where required.

- **ID**: `EP-3-8`
- **Slice**: `RecordDegreeObtained`
- **Type**: `Command`
- **Why now**: qualification maintenance can begin once academic and reference data exist.
- **Implementation tasks**:
  - Add qualification-record command and validator.
  - Reject duplicate academic-degree combinations.
  - Persist new qualification entries.
- **Definition of done**:
  - [ ] Duplicate academic-degree pairs are rejected.
  - [ ] New qualification is visible through qualification queries.
  - [ ] Domain and persistence rules are tested.

- **ID**: `EP-3-9`
- **Slice**: `AssignExtension`
- **Type**: `Command+Query`
- **Why now**: extension assignment depends on registered academics and provisioned extensions only.
- **Implementation tasks**:
  - Add assignment command and current-assignment query support.
  - Enforce unassigned extension requirement.
  - Back assignment with unique database constraint.
- **Definition of done**:
  - [ ] No extension can be assigned to more than one academic.
  - [ ] No academic can hold conflicting extension state.
  - [ ] Assignment flow is integration tested against concurrency-sensitive cases.

- **ID**: `EP-4-1`
- **Slice**: `RenewContract`
- **Type**: `Command`
- **Why now**: renewals require an existing contract from the earlier employment phase.
- **Implementation tasks**:
  - Add renewal command and validator.
  - Ensure academic is already contracted.
  - Replace end date with a future value.
- **Definition of done**:
  - [ ] Renewal fails when no contract exists.
  - [ ] Renewal enforces future-dated end dates.
  - [ ] Contract report source data updates correctly.

- **ID**: `EP-4-2`
- **Slice**: `ConvertContractToTenure`
- **Type**: `Command`
- **Why now**: conversion requires the contract path to exist first.
- **Implementation tasks**:
  - Add conversion command and handler.
  - Clear contract end date as tenure is applied.
  - Revalidate XOR employment rule after transition.
- **Definition of done**:
  - [ ] Conversion only succeeds from contracted state.
  - [ ] Contract end date is cleared on success.
  - [ ] Query outputs show tenured state only.

- **ID**: `EP-4-3`
- **Slice**: `UpdateDegreeUniversity`
- **Type**: `Command`
- **Why now**: qualification updates depend on recorded qualifications existing first.
- **Implementation tasks**:
  - Add update command and validator.
  - Resolve current qualification record.
  - Persist the new university linkage.
- **Definition of done**:
  - [ ] Updates fail when the qualification record is missing.
  - [ ] Updated university is visible in qualification queries.
  - [ ] Command path is covered by integration tests.

- **ID**: `EP-4-4`
- **Slice**: `RemoveDegreeRecord`
- **Type**: `Command`
- **Why now**: qualification removal requires earlier qualification creation and domain guards.
- **Implementation tasks**:
  - Add remove-degree command and handler.
  - Enforce minimum-one-qualification rule.
  - Verify downstream query consistency after removal.
- **Definition of done**:
  - [ ] Removing the last qualification is rejected.
  - [ ] Remaining qualifications stay queryable.
  - [ ] Rule coverage exists at integration level.

- **ID**: `EP-4-5`
- **Slice**: `ListQualifications`
- **Type**: `Query`
- **Why now**: listing qualifications depends on recorded qualification data first.
- **Implementation tasks**:
  - Add queries by academic, degree, and university.
  - Support stable filters and pagination where needed.
  - Verify returned qualification projections.
- **Definition of done**:
  - [ ] All three list modes return correct qualification sets.
  - [ ] Filters align with persisted reference data.
  - [ ] Query paths are integration tested.

- **ID**: `EP-4-6`
- **Slice**: `ReassignExtension`
- **Type**: `Command`
- **Why now**: reassignment requires an established assignment flow to exist first.
- **Implementation tasks**:
  - Add reassign command and handler.
  - Release current extension before applying the new one atomically.
  - Preserve 1:1 uniqueness through the transition.
- **Definition of done**:
  - [ ] Source and target extension state remain consistent after reassignment.
  - [ ] Atomic transition protects against duplicate assignment.
  - [ ] Integration tests cover failure rollback.

- **ID**: `EP-4-7`
- **Slice**: `ReleaseExtension`
- **Type**: `Command`
- **Why now**: release depends on prior extension assignment state.
- **Implementation tasks**:
  - Add release command and handler.
  - Return released extension to available pool.
  - Verify academic state is updated.
- **Definition of done**:
  - [ ] Released extensions are queryable as available.
  - [ ] Academic no longer references the released extension.
  - [ ] Release path is integration tested.

- **ID**: `EP-4-8`
- **Slice**: `ListAvailableExtensions`
- **Type**: `Query`
- **Why now**: availability reporting becomes meaningful after assignment and release flows exist.
- **Implementation tasks**:
  - Add available-extension query.
  - Filter to provisioned, unassigned extensions only.
  - Verify results after assign, reassign, and release flows.
- **Definition of done**:
  - [ ] Query excludes assigned extensions.
  - [ ] Query reflects released and newly provisioned extensions.
  - [ ] Integration tests cover state transitions.

- **ID**: `EP-5-1`
- **Slice**: `DeregisterAcademic`
- **Type**: `Command`
- **Why now**: deregistration depends on prior registration and extension lifecycle behavior.
- **Implementation tasks**:
  - Add deregistration command and handler.
  - Release active extension and emit domain event.
  - Preserve retained historical qualification data.
- **Definition of done**:
  - [ ] Deregistration fails cleanly for missing academics.
  - [ ] Assigned extension is released during flow.
  - [ ] Event consumers have what they need for reports and audit trails.

- **ID**: `EP-6-1`
- **Slice**: `AcademicDirectory`
- **Type**: `Query`
- **Why now**: it is the baseline report once academic registration and updates are stable.
- **Implementation tasks**:
  - Build read-optimized directory query.
  - Include rank, access level, extension, and employment status.
  - Validate paging and sorting.
- **Definition of done**:
  - [ ] Directory output matches underlying academic state.
  - [ ] Query is performant on seeded data.
  - [ ] Report tests pass.

- **ID**: `EP-6-2`
- **Slice**: `ByRankReport`
- **Type**: `Query`
- **Why now**: it depends on registration data and rank mutations being complete.
- **Implementation tasks**:
  - Build rank-grouped query and counts.
  - Include derived access level in output.
  - Verify updates after rank changes.
- **Definition of done**:
  - [ ] Counts and listing by rank are accurate.
  - [ ] Derived access levels align with current rank.
  - [ ] Report is integration tested.

- **ID**: `EP-6-3`
- **Slice**: `ByAccessLevelReport`
- **Type**: `Query`
- **Why now**: it depends specifically on rank-driven access-level derivation being stable.
- **Implementation tasks**:
  - Build access-level grouped query and counts.
  - Read from current rank-derived state only.
  - Verify changes after rank mutations.
- **Definition of done**:
  - [ ] P, SL, and L map to INT, NAT, and LOC consistently.
  - [ ] Counts and listings are accurate.
  - [ ] Query is covered by projection and integration tests.

- **ID**: `EP-6-4`
- **Slice**: `TenuredAcademicsReport`
- **Type**: `Query`
- **Why now**: it depends on tenure operations being fully validated first.
- **Implementation tasks**:
  - Build tenured-academic query.
  - Include rank and qualification summary.
  - Verify transitions from contract conversion.
- **Definition of done**:
  - [ ] Only tenured academics appear.
  - [ ] Qualification summaries are accurate.
  - [ ] Query is integration tested after employment transitions.

- **ID**: `EP-6-5`
- **Slice**: `ContractedAcademicsReport`
- **Type**: `Query`
- **Why now**: it depends on contract assignment and renewal flows being stable.
- **Implementation tasks**:
  - Build contracted-academic query.
  - Sort by contract end date ascending.
  - Verify renewed dates are reflected.
- **Definition of done**:
  - [ ] Only currently contracted academics appear.
  - [ ] Sort order is stable and correct.
  - [ ] Query is integration tested.

- **ID**: `EP-6-6`
- **Slice**: `ExpiringContractsReport`
- **Type**: `Query`
- **Why now**: it requires current contract data and renewal behavior to be accurate first.
- **Implementation tasks**:
  - Build date-window query with configurable threshold.
  - Add default 90-day filter behavior.
  - Verify boundary-date handling.
- **Definition of done**:
  - [ ] Default and custom thresholds return expected rows.
  - [ ] Past-due and future window behavior is correct.
  - [ ] Query is covered by date-focused tests.

- **ID**: `EP-6-7`
- **Slice**: `QualificationReports`
- **Type**: `Query`
- **Why now**: grouped qualification reports require qualification creation and maintenance slices to be complete.
- **Implementation tasks**:
  - Build grouped reports by degree and by university.
  - Add counts and listing projections.
  - Verify updates after qualification changes.
- **Definition of done**:
  - [ ] Degree-grouped and university-grouped outputs are accurate.
  - [ ] Counts stay synchronized after add, update, and remove flows.
  - [ ] Report tests pass.

- **ID**: `EP-6-8`
- **Slice**: `AccessLevelDistributionReport`
- **Type**: `Query`
- **Why now**: it depends on final rank-driven access-level state and is purely analytical.
- **Implementation tasks**:
  - Build grouped access-level distribution query.
  - Verify distribution updates after rank changes and deregistration.
  - Validate output shape for dashboards.
- **Definition of done**:
  - [ ] Distribution totals match active academics.
  - [ ] Derived access-level logic is preserved in report output.
  - [ ] Query is integration tested.

## Validation and Quality Gates

### Business-rule gates

- [ ] ExclusiveOr employment rule is covered by unit and integration tests for `GrantTenure`, `AssignContract`, `RenewContract`, `ConvertContractToTenure`, and `RemoveEmploymentStatus`.
- [ ] AccessLevel derivation from Rank is verified for `P -> INT`, `SL -> NAT`, and `L -> LOC` in aggregate and report tests.
- [ ] Academic creation and degree removal both preserve the rule that an academic must retain at least one qualification.
- [ ] Extension assignment, reassignment, release, and deregistration preserve 1:1 uniqueness between Academic and Extension.
- [ ] Contract dates are validated as future-dated in `AssignContract` and `RenewContract`.

### Phase-end quality gates

- [ ] Phase 0: Shared Kernel tests and migration checks pass before any slice starts.
- [ ] Phase 1: reference data and extension provisioning tests pass before `RegisterAcademic` starts.
- [ ] Phase 2: registration integration tests pass before any dependent slice starts.
- [ ] Phase 3: employment, rank, qualification-add, and extension-assign tests pass before dependent sequential slices start.
- [ ] Phase 4: contract conversion, qualification maintenance, and extension lifecycle tests pass before deregistration or reporting begins.
- [ ] Phase 5: deregistration flow and event publication tests pass before reports finalize.
- [ ] Phase 6: report accuracy, filtering, pagination, and performance checks pass before release.

### Release readiness checks

- [ ] All slices from the implementation plan are mapped to a phase.
- [ ] No phase starts before its declared blockers are complete.
- [ ] API and query paths meet project testing and performance targets.
- [ ] Required validators, database constraints, and integration tests exist for each domain rule.

## Risks and Mitigations

| Risk                                                                  | Impact | Mitigation                                                                                                                 |
| --------------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------------------------------- |
| Shared Kernel changes late in delivery ripple across every slice      | High   | Freeze aggregate contracts after Phase 0 and require regression tests before changes merge.                                |
| `RegisterAcademic` slips and blocks most of the backlog               | High   | Treat Phase 2 as the top delivery milestone and avoid parallel work on dependent slices until it passes integration tests. |
| Employment-state bugs break XOR rules                                 | High   | Centralize guards in the aggregate and verify all employment commands against the same invariant tests.                    |
| Extension uniqueness breaks under concurrent updates                  | High   | Add database uniqueness constraints and integration tests for assignment, reassignment, release, and deregistration.       |
| Qualification maintenance accidentally removes the last qualification | Medium | Enforce the rule in both validators/handlers and regression tests around removal flows.                                    |
| Report slices drift from source-of-truth behavior                     | Medium | Build reports only after source commands are stable and verify grouped results against seeded scenarios.                   |
| Query performance degrades as report volume grows                     | Medium | Use read-optimized queries, indexes, seeded performance tests, and pagination from the start of Phase 6.                   |

## Exit Criteria

- [ ] Shared Kernel, all 24 named slices, and all report slices from the implementation plan are implemented.
- [ ] Every slice is in the phase required by the declared dependency graph.
- [ ] `RegisterAcademic` is complete and validated before any dependent slice ships.
- [ ] All required business rules are covered by passing automated tests.
- [ ] Report slices are implemented only after their source data producers are complete.
- [ ] The backlog is executable phase by phase without reworking dependency order.
- [ ] Release readiness checks pass for validation, performance, and traceable rule coverage.

A team can exit this plan only when all phase gates are satisfied, every slice has met its definition of done, and no dependency or business-rule validation remains unresolved.

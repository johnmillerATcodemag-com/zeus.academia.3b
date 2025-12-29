---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-ai-dev-process-generation"
prompt: |
  submit the #file:create-ai-dev-process.prompt.md
started: "2025-12-28T19:30:00Z"
ended: "2025-12-28T19:40:00Z"
task_durations:
  - task: "design workflow structure"
    duration: "00:03:00"
  - task: "draft directives"
    duration: "00:05:00"
  - task: "optimize tokens"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-ai-dev-process-generation/conversation.md"
source: ".github/prompts/create-ai-dev-process.prompt.md"
description: "AI-assisted software development workflow and review process"
applyTo: "**"
---

# AI-Assisted Software Development Process

## AI Code Generation

**When to Use:**

- Boilerplate, repetitive patterns, standard implementations
- Initial scaffolding, test skeletons, documentation templates
- Code transformations, refactoring, format conversions

**Requirements:**

- MUST include complete provenance metadata (see ai-assisted-output.instructions.md)
- MUST pass existing tests or include new passing tests
- MUST follow project style, patterns, and conventions
- MUST document non-obvious logic, edge cases, assumptions
- MUST reference source prompt or instruction file

**Prohibited Without Review:**

- Security-critical code (auth, crypto, permissions)
- Database schema changes, data migrations
- API contract modifications, breaking changes
- Production configuration, environment variables

## AI Code Reviews

**Required For:**

- All AI-generated code before commit
- All human code in PR before merge
- Any security-sensitive changes
- Cross-cutting refactors

**Review Focus:**

- **Correctness**: Logic errors, edge cases, type safety
- **Security**: Injection risks, auth bypasses, data exposure
- **Performance**: N+1 queries, memory leaks, blocking operations
- **Style**: Naming, structure, idioms, conventions
- **Tests**: Coverage, quality, edge cases

**Tools:**

- Use `review` tool for branch comparisons
- Use `reviewUnstaged` for working directory changes
- Use `reviewStaged` for pre-commit validation

**Interpretation:**

- High severity → MUST fix before commit
- Medium severity → Fix or document reason to skip
- Low severity → Consider for future improvement

## Human Code Reviews

**Mandatory For:**

- First use of new patterns, libraries, or architectures
- Security-critical changes (auth, permissions, data access)
- API contracts, database schemas, breaking changes
- Code flagged by AI review as high-risk or complex
- Any change touching >500 lines or >5 files

**Review Criteria:**

- [ ] Solves stated problem completely
- [ ] No unintended side effects or regressions
- [ ] Tests validate success and failure cases
- [ ] Documentation updated (README, API docs, comments)
- [ ] Follows project conventions and standards
- [ ] No security vulnerabilities introduced
- [ ] Performance implications acceptable

**Process:**

- AI review first → address findings → human review
- Reviewer requests changes → author updates → re-review
- Approved → ready for merge (pending PR approval)

## PR Approval Workflow

**Approval Gates:**

1. **AI Review Pass** (automated)

   - No high-severity issues unresolved
   - All required tests passing
   - Provenance metadata complete (AI-generated code)

2. **Human Approval** (1+ required)
   - Maintainer or code owner review
   - Approval indicates: correct, safe, maintainable, tested

**Who Can Approve:**

- Project maintainers: all PRs
- Code owners: files in their domain
- Senior devs: routine changes in their area

**Merge Requirements:**

- [ ] **Technical Gates**: All checks in [Git Workflow](git-workflow.instructions.md) passed
- [ ] **AI Review**: Completed with no unresolved high-severity issues
- [ ] **Human Approval**: ≥1 approval from qualified reviewer
- [ ] **Comments**: No unresolved review comments

**Special Cases:**

- Hotfix: 1 approval, AI review optional if critical
- Docs-only: AI review optional, 1 approval sufficient
- Bot/automation: Requires maintainer approval

## Quality Gates

**Technical Gates:**
Refer to [Git Workflow](git-workflow.instructions.md) for all CI, testing, and metadata requirements.

**Process Gates:**
- [ ] AI Review completed
- [ ] Human Review completed
- [ ] PR Approval granted

## Exceptions

**Emergency Hotfix:**

- May skip AI review if immediate deployment critical
- Requires post-merge review and follow-up PR if needed
- Document exception reason in commit message

**Experimental Branches:**

- Relaxed review requirements
- Must not merge to main/production branches
- Label as `experimental` or `wip`

**Automated Updates:**

- Dependency bumps: AI review + 1 approval
- Generated code (schemas, clients): Validate generation, spot-check output

## Integration

After creating this file, update `.github/instructions/project-overview.instructions.md`:

- Add reference in "Standards" or "Key Patterns" section
- Link as: `[AI-Assisted Development Process](.github/instructions/ai-dev-process.instructions.md)`
- Note: Defines code generation, review, and approval workflows

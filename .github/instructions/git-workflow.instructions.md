---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-git-workflow-generation"
prompt: |
  subit prompt #file:create-git-workflow-instructions.prompt.md
started: "2025-12-28T20:00:00Z"
ended: "2025-12-28T20:15:00Z"
task_durations:
  - task: "analyze prompt requirements"
    duration: "00:02:00"
  - task: "structure workflow sections"
    duration: "00:08:00"
  - task: "optimize token usage"
    duration: "00:03:00"
  - task: "validate metadata"
    duration: "00:02:00"
total_duration: "00:15:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-git-workflow-generation/conversation.md"
source: ".github/prompts/create-git-workflow-instructions.prompt.md"
description: "Git workflow, branching, PR requirements, and quality gates for AI-assisted development"
applyTo: "**"
---

# Git Workflow

## Trunk-Based Development

**Main Branch**: Single source of truth, always deployable
**Feature Branches**: Max 2-3 days lifespan, merge or abandon
**Integration**: Commit to main frequently, minimum daily

### Branch Creation Rules

**Create Branch When:**

- Multi-file changes requiring review
- Experimental or uncertain work
- AI-generated code requiring validation
- Changes touching >5 files or >200 lines

**Direct to Main When:**

- Docs-only updates
- Single-file fixes (<50 lines)
- Metadata corrections
- README updates

### Branch Naming

**Standard**: `<type>/<short-description>`

- `feature/add-auth`
- `fix/memory-leak`
- `docs/api-reference`

**AI-Generated**: `ai/<chat-id>-<description>`

- `ai/2025-12-28-refactor-parser`
- `ai/chat-001-add-validation`

### Merge Strategy

**Prefer**: Fast-forward (rebase before merge)
**Avoid**: Merge commits (except cross-branch integrations)
**Require**: Linear history on main

## AI-Generated Code Branching

### Branch Metadata

**Commit Messages** must reference AI provenance:

```
<type>: <description>

AI-generated via chat: <chat-id>
Model: <provider>/<model>@<version>
Logs: ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/
```

**First Commit** in AI branch should link conversation log:

```
chore: initialize AI development session

Chat ID: <chat-id>
Conversation: ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/conversation.md
Summary: ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/summary.md
```

### Multi-Session Development

**Same Chat ID**: Continue on same branch or reference in new branch
**Different Sessions**: New `chat-id`, link to previous in summary.md
**Handoff**: Document context in conversation.md for human continuation

### Orphan Cleanup

**Weekly Review**: Delete merged AI branches older than 7 days
**Abandoned Work**: Delete unmerged branches older than 14 days after confirmation
**Archive Logs**: Retain all conversation logs regardless of branch status

## PR Requirements (AI-Generated Code)

### Metadata Validation

**File-Level Checks:**

- [ ] Front matter or sidecar exists for all modified files
- [ ] Required fields: `ai_generated`, `model`, `operator`, `chat_id`, `prompt`, `started`, `ended`, `task_durations`, `total_duration`, `ai_log`, `source`
- [ ] `chat_id` matches conversation log filename pattern
- [ ] `ai_log` path exists and contains `conversation.md` + `summary.md`
- [ ] `model` format: `<provider>/<model>@<version>`
- [ ] Timestamps are valid ISO8601
- [ ] Durations sum to `total_duration`

**Repository-Level Checks:**

- [ ] README updated for durable artifacts (new files intended for long-term use)
- [ ] Conversation logs committed under `ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/`
- [ ] Summary references all artifacts generated

### Content Review

**Code Quality:**

- [ ] No obvious bugs or logic errors
- [ ] Follows project conventions (naming, structure, patterns)
- [ ] Security best practices applied (no hardcoded secrets, SQL injection risks)
- [ ] Error handling appropriate for context
- [ ] Performance acceptable (no N+1 queries, excessive allocations)

**Test Coverage:**

- [ ] Tests added for new functionality
- [ ] Tests updated for modified behavior
- [ ] Test coverage ≥80% for new code
- [ ] Edge cases covered
- [ ] Tests pass locally and in CI

**Documentation:**

- [ ] Complex logic has inline comments explaining "why"
- [ ] Public APIs documented (docstrings, README)
- [ ] Breaking changes noted in PR description
- [ ] Usage examples provided for new features

**Privacy & Security:**

- [ ] No PII, credentials, or sensitive data in code or logs
- [ ] Conversation logs sanitized (no API keys, passwords)
- [ ] External dependencies vetted for known vulnerabilities

### PR Template

**Title Format**: `[AI] <type>: <description> (#<chat-id>)`

**Required Sections:**

```markdown
## Summary

<What changed and why>

## AI Provenance

- **Chat ID**: <chat-id>
- **Model**: <provider>/<model>@<version>
- **Operator**: <github-username>
- **Logs**: [Conversation](../ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/conversation.md) | [Summary](../ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/summary.md)

## Changes

- <file>: <purpose>

## Testing

- [ ] Existing tests pass
- [ ] New tests added
- [ ] Manual testing performed

## Review Focus

<Areas needing extra attention>

## Checklist

- [ ] Metadata complete
- [ ] Logs committed
- [ ] Tests passing
- [ ] README updated (if applicable)
```

## Quality Gates (Main Branch)

**Technical & CI Requirements:**

- [ ] All CI checks pass (tests, linting, security scans)
- [ ] AI provenance validation passes (automated check)
- [ ] Test coverage ≥80% overall, ≥80% for changed lines
- [ ] No merge conflicts with main
- [ ] Linear history (rebase completed)
- [ ] Documentation builds without errors
- [ ] Security scans clear (no high/critical vulnerabilities)
- [ ] Commit messages follow conventional commits
- [ ] All commits signed (if GPG signing enabled)

**Process Requirements:**

- See [AI-Assisted Development Process](ai-dev-process.instructions.md) for approval workflows and human review criteria.

**Automated Enforcement:**

- Branch protection rules prevent direct pushes
- Status checks must pass before merge button enabled
- Stale PRs auto-closed after 30 days inactivity

**Post-Merge Actions:**

- Delete merged branch automatically
- Update project board/issue tracker
- Trigger deployment pipeline (if applicable)

## Quality Gates (PRs)

**Before Review Request:**

- [ ] Branch updated with latest main (rebase or merge)
- [ ] All commits signed (if repository requires)
- [ ] Commit messages use conventional format: `<type>(<scope>): <subject>`
- [ ] AI-generated files include complete metadata
- [ ] Tests added/updated for all functional changes
- [ ] No secrets, credentials, or PII in diff
- [ ] Linting passes (`eslint`, `pylint`, `clippy`, etc.)
- [ ] Type checking passes (TypeScript, mypy, etc.)
- [ ] Breaking changes documented in PR description and CHANGELOG

**Review Readiness:**

- [ ] PR description complete with summary, testing, and checklist
- [ ] Requested reviewers assigned (code owners auto-assigned)
- [ ] Labels applied (`ai-generated`, `breaking-change`, etc.)
- [ ] Linked to related issues/tickets
- [ ] Estimated review time noted (small/medium/large)

**During Review:**

- Address feedback within 48 hours
- Re-request review after pushing changes
- Resolve or respond to all review comments
- Keep PR scope focused (split if exceeding 500 lines)

## Exception Handling

**Hotfixes to Main:**

- Allowed for critical production issues
- Require post-merge PR with retroactive review
- Must include incident report in PR description

**Metadata-Incomplete Merges:**

- Block merge via CI check
- Provide remediation instructions in check output
- Allow override by repository admin with justification

**Large Refactors:**

- May exceed 500-line guideline
- Require ≥2 approvals
- Consider incremental PRs if possible

## Integration with AI Policy

After creating this instruction file, update `.github/instructions/project-overview.instructions.md` to reference this Git workflow in the "Standards" or "Development Process" section:

```markdown
- **Git Workflow**: [Trunk-based development, AI branching, PR requirements](.github/instructions/git-workflow.instructions.md)
```

Add this reference under existing standards entries, maintaining alphabetical or logical grouping.

## Enforcement

**CI Pipeline** must include:

- `verify-ai-provenance` job: Check metadata completeness
- `test-coverage` job: Enforce 80% threshold
- `lint` job: Run configured linters
- `security-scan` job: Check dependencies for vulnerabilities
- `docs-build` job: Validate documentation compiles

**PR Template Auto-Population:**

- Use `.github/pull_request_template.md` with AI provenance section
- Auto-detect AI-generated files and pre-fill metadata summary

**Branch Protection Settings:**

- Require 1+ approval from code owners
- Require all status checks to pass
- Dismiss stale approvals on new push
- Enforce linear history
- Prevent force pushes
- Delete merged branches automatically

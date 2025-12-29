---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-git-workflow-prompt"
prompt: |
  Create a prompt to create a Git Workflow instruction file. The instructions should:
  - Include guidance for trunk-based development.
  - Branching strategies for AI generated code.
  - PR requirements for AI generated code.
  - Quality gates for the main branch.
  - Quality gates for PRs.
  IMPORTANT: The instruction file generated from this prompt should contain an instruction
  to update the Project Overview instruction file with a reference to the generated
  Git Workflow instruction file.
started: "2025-12-28T00:00:00Z"
ended: "2025-12-28T00:05:00Z"
task_durations:
  - task: "analyze requirements"
    duration: "00:01:00"
  - task: "structure prompt"
    duration: "00:03:00"
  - task: "add metadata"
    duration: "00:01:00"
total_duration: "00:05:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-git-workflow-prompt/conversation.md"
source: "johnmillerATcodemag-com"
description: "Prompt for generating Git workflow instruction file"
context: "Repository with AI-assisted development, trunk-based workflow"
expected_output: "Instruction file with Git workflow, branching, PR, and quality gate guidance"
---

# Prompt: Create Git Workflow Instruction File

Generate a comprehensive `.instructions.md` file at `.github/instructions/git-workflow.instructions.md` that defines Git workflow practices for this repository.

## Required Content

### 1. Trunk-Based Development

- Main branch as single source of truth
- Short-lived feature branches (max 2-3 days)
- Continuous integration to main
- Fast-forward merge preference
- Branch naming conventions
- When to create branches vs. direct commits

### 2. Branching Strategy for AI-Generated Code

- Branch naming: `ai/<chat-id>-<description>`
- Required branch metadata in commit messages
- Linking branches to AI conversation logs
- Handling multi-session AI development
- Orphan branch cleanup procedures

### 3. PR Requirements for AI-Generated Code

- **Metadata Validation**:
  - All files have complete front matter or sidecars
  - Valid `chat_id`, `ai_log`, `model`, `operator` fields
  - AI logs exist at referenced paths
  - README updated for notable artifacts
- **Content Review**:
  - Code quality and correctness
  - Test coverage requirements
  - Documentation completeness
  - Security and privacy checks
- **Template Structure**:
  - PR title format
  - Required description sections
  - AI provenance summary
  - Link to conversation logs

### 4. Quality Gates for Main Branch

- All CI checks must pass
- Code review approval required (specify count)
- AI provenance validation passes
- Test coverage thresholds met (specify %)
- No merge conflicts
- Linear history maintained
- Documentation builds successfully
- Security scans clear

### 5. Quality Gates for PRs

- Branch up to date with main
- All commits signed
- Conventional commit messages
- AI-generated files have metadata
- Tests added/updated for changes
- No sensitive data exposed
- Linting passes
- Breaking changes documented

## Additional Requirements

### Cross-Reference Instruction

Include a directive that instructs the AI to:

```
After creating this instruction file, update the Project Overview instruction file
(if it exists at `.github/instructions/project-overview.instructions.md`) to include
a reference to this Git workflow instruction file in its relevant section (e.g.,
"Development Process" or "Repository Standards").
```

### File Structure

- Complete YAML front matter with all required metadata fields
- `applyTo: "**"` glob pattern
- Clear section headings
- Actionable directives (use imperatives)
- Minimal examples only where necessary
- Token-optimized language

### Metadata Requirements

Include complete provenance metadata:

- `ai_generated: true`
- `model` (copy from UI)
- `operator` (GitHub username)
- `chat_id` (unique identifier)
- `prompt` (capture exact request)
- Timestamps (`started`, `ended`)
- `task_durations` list
- `total_duration`
- `ai_log` path
- `source`
- `description` (one-line purpose)
- `applyTo: "**"`

## Success Criteria

The generated instruction file should:

- Provide unambiguous Git workflow guidance
- Integrate AI provenance requirements into Git processes
- Define clear, testable quality gates
- Support both manual and automated enforcement
- Maintain token efficiency (remove redundancy)
- Follow repository instruction file conventions
- Include the cross-reference update directive

## Context

This repository follows the AI-assisted output policy defined in
`.github/instructions/ai-assisted-output.instructions.md`. All AI-generated
artifacts require complete provenance metadata and conversation logs.

Git workflow must enforce these standards while supporting efficient
trunk-based development with minimal friction.

---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-project-overview-generation"
prompt: |
  submit the #file:create-project-overview.prompt.md
started: "2025-12-28T19:00:00Z"
ended: "2025-12-28T19:10:00Z"
task_durations:
  - task: "analyze repository structure"
    duration: "00:03:00"
  - task: "generate overview content"
    duration: "00:05:00"
  - task: "optimize tokens"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-project-overview-generation/conversation.md"
source: ".github/prompts/create-project-overview.prompt.md"
description: "zeus.academia.3b - AI-assisted development workflow framework"
applyTo: "**"
---

# Project: Zeus Academia 3B

**Purpose**: Experimental framework for AI-assisted development workflows, prompt engineering, and instruction management
**Type**: Development framework / research project
**Stack**: Markdown + YAML (documentation-centric)

## Architecture

- **Structure**: Documentation-based configuration system
- **Core Dirs**:
  - `.github/instructions/` – AI assistant instruction files
  - `.github/prompts/` – Reusable prompt templates
  - `.github/chatmodes/` – Specialized chatmode configurations
  - `.github/models/` – Model configurations
  - `ai-logs/` – Conversation and session logs
- **Build**: None (documentation artifacts)

## Standards

- **Format**: Markdown with YAML front matter
- **Naming**:
  - Instructions: `<purpose>.instructions.md`
  - Prompts: `<purpose>.prompt.md`
  - Chatmodes: `<purpose>.chatmode.md`
- **Metadata**: Complete AI provenance required (see ai-assisted-output.instructions.md)
- **Structure**: Token-optimized, directive-based
- **Tests**: Manual validation, CI checks for metadata completeness
- **Workflows**:
  - [AI Development Process](ai-dev-process.instructions.md)
  - [Git Workflow](git-workflow.instructions.md)

## Environment

- Target: AI assistants (GitHub Copilot, Claude, GPT-4)
- Deploy: Git repository (no runtime)
- Dependencies: None (self-contained)

## Constraints

- **Token Efficiency**: Minimize verbosity in all artifacts
- **Provenance**: All AI-generated content must have complete metadata
- **Auditability**: Conversation logs required for AI-assisted work
- **Glob Patterns**: `applyTo` field must use valid glob syntax
- **File Organization**: Hierarchical date-based structure for logs (`yyyy/mm/dd`)

## Key Patterns

- **Instructions**: Directive-based rules for AI assistants, applied via `applyTo` glob
- **Prompts**: Reusable templates with arguments (`{{variable}}`)
- **Chatmodes**: Specialized personas/workflows for specific tasks
- **Logs**: Full conversation + summary per chat session

## Critical Files

- `.github/instructions/ai-assisted-output.instructions.md` – Provenance policy (required reading)
- `.github/instructions/ai-dev-process.instructions.md` – Development workflow and review process
- `.github/instructions/git-workflow.instructions.md` – Branching, PRs, and quality gates
- `.github/instructions/instruction-file-generation.instructions.md` – Instruction authoring guide
- `.github/instructions/prompt-file-generation.instructions.md` – Prompt authoring guide
- `.github/instructions/chatmode-file-generation.instructions.md` – Chatmode authoring guide

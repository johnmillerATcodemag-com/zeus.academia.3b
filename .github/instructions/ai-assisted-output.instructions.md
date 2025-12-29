---
ai_generated: true
model: "anthropic/claude-3.5-sonnet@2024-10-22"
operator: "johnmillerATcodemag-com"
chat_id: "ai-assisted-output-policy-2025-10-15"
prompt: |
  Create comprehensive AI provenance and logging policy for all AI-assisted
  outputs in the repository, defining required metadata, workflow, and enforcement.
started: "2025-10-15T13:00:00Z"
ended: "2025-10-15T13:45:00Z"
task_durations:
  - task: "policy design"
    duration: "00:20:00"
  - task: "workflow specification"
    duration: "00:15:00"
  - task: "template creation"
    duration: "00:10:00"
total_duration: "00:45:00"
ai_log: "ai-logs/2025/10/15/ai-assisted-output-policy-2025-10-15/conversation.md"
source: ".github/prompts/create-ai-assisted-output-instructions.prompt.md"
applyTo: "**/*"
---

# AI-Assisted Output Instructions

This policy keeps AI-generated artifacts auditable with minimal overhead. Follow it for all code, docs, diagrams, tests, or data touched by an AI assistant.

## Quick Obligations

- Always work inside an active chat with a unique `chat_id`; block output otherwise.
- Capture the exact `provider/model@version` shown by the tool before generating content.
- Embed provenance metadata (front matter or sidecar) including the raw prompt and timestamps.
- Store full chat logs under `ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/` with `conversation.md` and `summary.md`.
- Record task durations, keep README entries for notable new artifacts, and run the checklists below before committing.

## Metadata Placement

- Markdown or formats with front matter: embed YAML at the top of the file.
- Non-front-matter formats: create `<artifact>.meta.md` with the same fields.
- Use lowercase file paths; never create sidecars when front matter is possible.

## Canonical Metadata Fields

Every AI-assisted artifact **must** include:

- `ai_generated: true`
- `model: "<provider>/<model>@<version>"`
- `operator: "<github-username>"` (must be the GitHub username, e.g., "johnmillerATcodemag-com")
- `chat_id: "<chat-id>"`
- `prompt: |` + exact user prompt
- `started`, `ended` (ISO8601)
- `task_durations:` list of `{task, duration}` entries
- `total_duration`
- `ai_log: "ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/conversation.md"`
- `source:` creator or prompt file

**For instruction files (`.instructions.md`)**: Also include:

- `description: "<one-line purpose>"`
- `applyTo: "<glob pattern>"` (e.g., '**', 'src/**/_.py', '\*\*/_.bicep')

**For prompt files (`.prompt.md`)**: Also include:

- `description: "<one-line purpose>"`
- `context: "<relevant context or constraints>"`
- `expected_output: "<type of output expected>"`

**For chatmode files (`.chatmode.md`)**: Also include:

- `description: "<one-line purpose>"`
- `chatmode_type: "<type of chatmode>"` (e.g., 'coding', 'research', 'testing')
- `capabilities: []` list of chatmode capabilities

### Sample Front Matter

```yaml
---
ai_generated: true
model: "openai/gpt-4o@2024-11-20"
operator: "johnmillerATcodemag-com"
chat_id: "2025-04-18-refactor"
prompt: |
  Exact request that triggered the artifact.
started: "2025-04-18T17:03:11Z"
ended: "2025-04-18T17:06:54Z"
task_durations:
  - task: "draft"
    duration: "00:02:20"
  - task: "review"
    duration: "00:01:23"
total_duration: "00:03:43"
ai_log: "ai-logs/2025/04/18/2025-04-18-refactor/conversation.md"
source: "johnmillerATcodemag-com"
---
```

### Sample Front Matter (Instruction File)

```yaml
---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-meta-instructions"
prompt: |
  create an instruction file to guide an ai assistant in the generation
  of instruction files. optimize the instruction file to minimize the
  number of tokens required.
started: "2025-10-28T14:30:00Z"
ended: "2025-10-28T14:35:00Z"
task_durations:
  - task: "draft"
    duration: "00:03:00"
  - task: "optimize"
    duration: "00:02:00"
total_duration: "00:05:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-meta-instructions/conversation.md"
source: "johnmillerATcodemag-com"
description: "Guide for generating instruction files"
applyTo: "**/*.instructions.md"
---
```

### Sample Front Matter (Prompt File)

```yaml
---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-data-analysis"
prompt: |
  create a prompt template for data analysis tasks
started: "2025-10-28T15:00:00Z"
ended: "2025-10-28T15:05:00Z"
task_durations:
  - task: "draft"
    duration: "00:05:00"
total_duration: "00:05:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-data-analysis/conversation.md"
source: "johnmillerATcodemag-com"
description: "Prompt template for data analysis"
context: "Python data science workflows"
expected_output: "Analysis code and visualizations"
---
```

### Sample Front Matter (Chatmode File)

```yaml
---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-test-chatmode"
prompt: |
  create a chatmode configuration for automated testing
started: "2025-10-28T16:00:00Z"
ended: "2025-10-28T16:10:00Z"
task_durations:
  - task: "draft"
    duration: "00:07:00"
  - task: "validate"
    duration: "00:03:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-test-chatmode/conversation.md"
source: "johnmillerATcodemag-com"
description: "Automated testing chatmode configuration"
chatmode_type: "testing"
capabilities:
  - "unit testing"
  - "integration testing"
  - "coverage reporting"
---
```

**Model Identification Guide**:

- **GitHub Copilot Chat** (as of Oct 2025): Uses `anthropic/claude-sonnet-4.5@unknown` unless you can identify the exact version
- **OpenAI GPT-4o**: `openai/gpt-4o@2024-11-20` (or specific version)
- **Claude 3.5 Sonnet**: `anthropic/claude-3.5-sonnet@2024-10-22`
- **Claude Sonnet 4.5**: `anthropic/claude-sonnet-4.5@<version>` (use `@unknown` if version not shown)
- If UI shows marketing names like "GPT-5-Codex", map to the actual underlying model
- When version is not surfaced, use `@unknown`
- Track additional models used in the same chat inside `summary.md`

## Chat Workflow (Canonical)

1. **Start** a fresh chat → capture `chat_id`, operator, and model label immediately.
2. **Scaffold logs on first artifact** at `ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/` with:
   - `conversation.md`: full timestamped transcript.
   - `summary.md`: objectives, outcomes, models used, remaining work.
   - Optional `artifacts/` for files not checked into the repo.
3. **Generate artifact** only if chat context is active; inject metadata using the captured model string.
4. **Update logs** after each artifact; export current transcript, append deliverables list, and record pending actions.
5. **Close** by finalizing `summary.md`, ensuring task durations and README updates are complete.

### Minimal conversation.md

````markdown
# AI Conversation Log

- Chat ID: <chat-id>
- Operator: <github-username>
- Model: <provider>/<model>@<version>
- Started: <ISO8601>
- Ended: <ISO8601>
- Total Duration: <hh:mm:ss>

## Context

- Inputs: <files/constraints>
- Targets: <intended artifacts>

## Exchanges

[<timestamp>] <user>

```text
<prompt>
```

[<timestamp>] <provider>/<model>@<version>

```text
<assistant reply>
```

## Artifacts

- <path> – <one-line purpose>

## Next Steps

- [ ] <action>
````

### Minimal summary.md

````markdown
# Session Summary – <chat-id>

**Date**: <YYYY-MM-DD>
**Operator**: <github-username>
**Model**: <provider>/<model>@<version>
**Duration**: <hh:mm:ss>

## Objective

- <goal>

## Deliverables

1. `<path>` – <purpose>

## Decisions

- <decision>: <rationale>

## Follow-up

- [ ] <action>

## Metadata

```yaml
chat_id: <chat-id>
started: <ISO8601>
ended: <ISO8601>
total_duration: <hh:mm:ss>
models_used:
  - <provider>/<model>@<version>
artifacts_count: <int>
files_modified: <int>
```
````

## Placement & README

- Keep this file at `.github/instructions/ai-assisted-output.instructions.md`.
- For every notable new artifact, add a bullet in `README.md` linking to the artifact (and optional `ai_log`) under an “AI-Assisted Artifacts” section.
- Temporary scratch work still requires logs and metadata but may skip README updates.

## Quality Checklist

- [ ] Metadata complete and embedded (or sidecar when required).
- [ ] Model label copied verbatim from the chat UI.
- [ ] Prompt, timestamps, task durations, and totals recorded.
- [ ] `ai-logs/.../conversation.md` and `summary.md` exist and reference the artifact.
- [ ] README updated for new durable artifacts.
- [ ] No sensitive data captured.
- [ ] Tests/docs run or noted as pending.

## PR Checklist

- [ ] Every changed AI-generated file references a real `chat_id` and `ai_log`.
- [ ] All logs committed alongside artifacts.
- [ ] README entries present where required.
- [ ] Provenance fields pass CI (formatter, YAML syntax, existing paths).

## Copilot / Tooling Requirements

- Auto-create chat IDs, log folders, and metadata scaffolds on first artifact.
- Prevent artifact generation without live chat context.
- Export transcripts and summaries automatically.
- Surface the active model name/version so it can be copied as `provider/model@version`.
- Track files generated per chat to support audits.

## Enforcement

- CI script `Verify AI Provenance` checks Markdown front matter for `ai_generated`, `chat_id`, `ai_log`, and valid log paths. Extend similarly for other formats as needed.

## Remediation

- Missing metadata → add required fields and regenerate logs.
- Orphaned artifacts → create `ai-logs/...` records retroactively and update README.
- Sidecar misuse → move metadata into front matter when supported.

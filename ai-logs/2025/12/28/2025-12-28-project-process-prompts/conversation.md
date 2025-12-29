# AI Conversation Log

- Chat ID: 2025-12-28-project-process-prompts
- Operator: johnmillerATcodemag-com
- Model: anthropic/claude-sonnet-4.5@unknown
- Started: 2025-12-28T00:00:00Z
- Ended: 2025-12-28T00:10:00Z
- Total Duration: 00:10:00

## Context

- Inputs: User request to create prompt for project overview instruction file
- Targets: `.github/prompts/create-project-overview.prompt.md`

## Exchanges

[2025-12-28T00:00:00Z] user

```text
Create a prompt to create an Project Overview instruction file containing the high level context for the project. The instruction file should be optimized for AI agents and to minimize the token requirements.
```

[2025-12-28T00:05:00Z] anthropic/claude-sonnet-4.5@unknown

```text
Reviewed existing prompt file generation instructions and examples to understand proper format and structure. Analyzed check-context.prompt.md as a reference for metadata and content organization.
```

[2025-12-28T00:10:00Z] anthropic/claude-sonnet-4.5@unknown

```text
Created create-project-overview.prompt.md with:
- Complete metadata including AI provenance and Copilot-specific fields
- Arguments for project details (name, type, language, technologies, purpose)
- Structured sections covering project identity, architecture, standards, environment, and constraints
- Token-optimization guidance
- Example structure template
- Validation checklist and anti-patterns
```

## Artifacts

- `.github/prompts/create-project-overview.prompt.md` – Prompt template for generating token-optimized project overview instruction files

## Next Steps

- [x] Create prompt file with complete metadata
- [x] Include argument placeholders for project details
- [x] Define output structure and optimization rules
- [x] Add validation checklist
- [x] Create conversation and summary logs

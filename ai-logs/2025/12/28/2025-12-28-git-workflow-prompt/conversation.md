# AI Conversation Log

- Chat ID: 2025-12-28-git-workflow-prompt
- Operator: johnmillerATcodemag-com
- Model: anthropic/claude-sonnet-4.5@unknown
- Started: 2025-12-28T00:00:00Z
- Ended: 2025-12-28T00:05:00Z
- Total Duration: 00:05:00

## Context

- Repository: zeus.academia.3b
- Branch: Part-Three
- Task: Create prompt file for Git workflow instruction generation

## Request

User requested creation of a prompt file that would guide an AI to generate a Git workflow instruction file covering:

- Trunk-based development guidance
- Branching strategies for AI-generated code
- PR requirements for AI-generated code
- Quality gates for main branch and PRs
- Cross-reference update to Project Overview file

## Exchanges

[2025-12-28T00:00:00Z] User

```text
Create a prompt to create a Git Workflow instruction file. The instructions should:
- Include guidance for trunk-based development.
- Branching strategies for AI generated code.
- PR requirements for AI generated code.
- Quality gates for the main branch.
- Quality gates for PRs.

IMPORTANT: The instruction file generated from this prompt should contain an instruction
to update the Project Overview instruction file with a reference to the generated
Git Workflow instruction file.
```

[2025-12-28T00:01:00Z] anthropic/claude-sonnet-4.5@unknown

Created comprehensive prompt file with:

- Complete metadata front matter
- Structured requirements for all requested topics
- Token-optimized language
- Cross-reference update directive
- Integration with repository AI-assisted output policy

## Artifacts

- `.github/prompts/create-git-workflow-instructions.prompt.md` – Prompt for generating Git workflow instruction file

## Task Breakdown

1. Analyzed user requirements
2. Structured prompt with clear sections
3. Added complete metadata per repository standards
4. Included cross-reference directive
5. Created conversation log structure

## Next Steps

- [ ] User can execute this prompt to generate the Git workflow instruction file
- [ ] Generated instruction file will contain directive to update project overview
- [ ] AI logs completed and linked

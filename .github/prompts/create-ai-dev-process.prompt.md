---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-ai-dev-process-prompt"
prompt: |
  Create a prompt to create a AI Assisted Software Development Process instruction file.
  The instructions should include guidance for AI generated code, AI code reviews,
  Human Code reviews, and Human PR approvals. Be optimized for AI agents and to
  minimize the token requirements. The instruction file should contain an instruction
  to update the Project Overview instruction file with a reference to itself.
started: "2025-12-28T00:00:00Z"
ended: "2025-12-28T00:00:00Z"
task_durations:
  - task: "design prompt structure"
    duration: "00:00:00"
  - task: "draft prompt content"
    duration: "00:00:00"
  - task: "add metadata"
    duration: "00:00:00"
total_duration: "00:00:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-ai-dev-process-prompt/conversation.md"
source: "johnmillerATcodemag-com"
description: "Prompt for AI-Assisted Software Development Process instruction file"
context: "Software development workflow with AI assistance, code reviews, and PR approvals"
expected_output: "Token-optimized instruction file covering AI code generation, reviews, and approval workflows"
---

# Prompt: AI-Assisted Software Development Process Instructions

Create a token-optimized instruction file at `.github/instructions/ai-dev-process.instructions.md` that defines the software development workflow for AI-assisted projects.

## Requirements

### Scope

- **Target**: All code files and development workflows
- **applyTo**: `**`

### Core Content Areas

1. **AI Code Generation**

   - When to use AI for code generation
   - Quality requirements for AI-generated code
   - Mandatory metadata and provenance tracking
   - Testing requirements for AI-generated code
   - Documentation requirements

2. **AI Code Reviews**

   - When AI reviews are required vs optional
   - What AI should check (patterns, bugs, security, style)
   - How to interpret AI review feedback
   - Integrating AI reviews into workflow

3. **Human Code Reviews**

   - When human reviews are mandatory
   - Review checklist and criteria
   - Relationship between AI and human reviews
   - Escalation paths for complex issues

4. **PR Approval Process**
   - Required approvals (AI vs human)
   - Approval criteria and gates
   - Who can approve what
   - Merge requirements

### Optimization Requirements

- Use imperative directives ("Do X", "Require Y")
- Eliminate filler words
- Use bullet lists over prose
- Group related rules together
- Provide only essential examples
- Target 400-600 tokens for core content

### Structure Requirements

Organize as:

```
## AI Code Generation
[Token-efficient directives]

## AI Code Reviews
[Token-efficient directives]

## Human Code Reviews
[Token-efficient directives]

## PR Approval Workflow
[Token-efficient directives]

## Quality Gates
[Checklist format]

## Exceptions
[Minimal cases only]
```

### Metadata Requirements

Include complete YAML front matter:

- Standard provenance fields (ai_generated, model, operator, chat_id, etc.)
- `description`: One-line purpose
- `applyTo`: `**`
- All timestamps, durations, ai_log reference

### Critical Requirement

**MUST include this directive in the generated instruction file:**

```markdown
## Integration

After creating this file, update `.github/instructions/project-overview.instructions.md`:

- Add reference to this file in the "Development Workflow" or "Process" section
- Link as: `[AI-Assisted Development Process](.github/instructions/ai-dev-process.instructions.md)`
- Note: Defines code generation, review, and approval workflows
```

## Output Format

Generate a complete `.github/instructions/ai-dev-process.instructions.md` file with:

1. Complete YAML front matter
2. Token-optimized instruction content
3. Clear section headings
4. Actionable directives
5. Integration instruction for Project Overview update

## Success Criteria

- [ ] All four workflow areas covered (AI gen, AI review, human review, PR approval)
- [ ] Token count under 800 for main content
- [ ] All directives are imperative and testable
- [ ] Metadata complete and valid
- [ ] Integration instruction included
- [ ] No redundant content
- [ ] Clear workflow sequence

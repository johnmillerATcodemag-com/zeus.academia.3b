---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-instruction-guide"
prompt: |
  create an token optimized instruction file to guide an ai assistant in the generation of instruction files.
started: "2025-10-28T14:00:00Z"
ended: "2025-10-28T14:05:00Z"
task_durations:
  - task: "design"
    duration: "00:03:00"
  - task: "draft"
    duration: "00:02:00"
total_duration: "00:05:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-instruction-guide/conversation.md"
source: "johnmillerATcodemag-com"
description: "Token-optimized guide for generating instruction files"
applyTo: "**/*.instructions.md"
---

# Instruction File Generation

## Naming & Location

`<purpose>.instructions.md` in `.github/instructions/`

## Metadata Requirements

### AI Provenance (AI-generated files only)

See [ai-assisted-output.instructions.md](ai-assisted-output.instructions.md) for complete AI provenance metadata requirements.

**Required fields:** `ai_generated`, `model`, `operator`, `chat_id`, `prompt`, `started`, `ended`, `task_durations`, `total_duration`, `ai_log`, `source`

**Instruction-specific:** `description`, `applyTo`

### Copilot Instruction Metadata

**Required:** `name`, `description`
**Recommended:** `applyTo`, `tags: [...]`
**Optional:** `examples`, `version`, `author`

```yaml
name: "Python Testing Standards"
description: "Enforce pytest usage with clear naming and coverage rules"
tags: [testing, python, pytest]
applyTo: "tests/**/*.py"
examples:
  - prompt: "Write a unit test for add_user()"
    response: "Use pytest with descriptive test names and fixtures"
version: "1.0"
author: "John Miller"
```

**applyTo Patterns:**

- `"**"` - All files
- `"src/**/*.py"` - Python files in src
- `"**/*.bicep"` - All Bicep files
- `["tests/**", "specs/**"]` - Multiple paths

## Content Structure

1. **Title** - Clear, specific purpose
2. **Core Rules** - Imperative voice, one concept per bullet, prioritized (requirements > preferences > examples), grouped
3. **Examples** - Concrete implementations in code blocks, minimal
4. **Checklists** - Multi-step processes only

## Writing Rules

**Eliminate:** Redundant phrasing ("in order to") | Obvious statements | Excessive examples | Meta-commentary
**Compress:** Tables > prose | Lists > paragraphs | Code > descriptions
**Merge:** Related rules | Subclauses vs. bullets | Similar examples

## Checklist

- [ ] Metadata: AI provenance (if generated) + `name`/`description`
- [ ] `applyTo`: Glob matches intended scope
- [ ] Each rule: Actionable without interpretation
- [ ] No conflicting directives
- [ ] Examples: Non-obvious cases only
- [ ] Word count: <50% of draft

**Anti-Patterns:** "Please follow..." | Repeating AI knowledge | Vague directives ("be careful") | Over-documenting standards | Duplicate examples

**Token Target:** 200-600 typical | 1000 max (unless complex)

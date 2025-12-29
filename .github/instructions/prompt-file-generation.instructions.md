---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-prompt-guide"
prompt: |
  create an token optimized instruction file to guide an ai assistant in the generation of prompt files.

  Update: Add tools and mode as optional metadata fields.
started: "2025-10-28T14:10:00Z"
ended: "2025-10-28T16:20:00Z"
task_durations:
  - task: "design"
    duration: "00:03:00"
  - task: "draft"
    duration: "00:02:00"
  - task: "update metadata fields"
    duration: "00:05:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-prompt-guide/conversation.md"
source: "johnmillerATcodemag-com"
description: "Token-optimized guide for generating prompt files"
applyTo: "**/*.prompt.md"
---

# Prompt File Generation

## Naming & Location

`<purpose>.prompt.md` in `.github/prompts/` or context directory

## Metadata Requirements

### AI Provenance (AI-generated files only)

See [ai-assisted-output.instructions.md](ai-assisted-output.instructions.md) for complete AI provenance metadata requirements.

**Required fields:** `ai_generated`, `model`, `operator`, `chat_id`, `prompt`, `started`, `ended`, `task_durations`, `total_duration`, `ai_log`, `source`

**Prompt-specific:** `description`, `context`, `expected_output`

### Copilot Prompt Metadata

**Required:** `name` (unique ID)
**Recommended:** `description`, `author`, `tags: [...]`
**Optional:** `arguments`, `examples`, `context`, `expected_output`, `tools: [...]`, `mode`

> **Note:** `author` refers to the creator of the prompt template. `operator` (in AI provenance) refers to the user executing the AI session.

```yaml
name: generate-readme
description: Generate comprehensive README
author: John Miller
tags: [documentation, markdown]
arguments:
  - name: project_name
    description: Project name
  - name: language
    description: Programming language
examples:
  - input: "Generate README for Python app"
    output: "README with setup, usage, contribution sections"
context: "Python dev environment"
expected_output: "Markdown README"
tools: ["generate", "edit"]
mode: interactive
```

## Content Structure

1. **Title** - Clear task
2. **Context** - Background, constraints, dependencies
3. **Instructions** - Direct objective, success criteria, format, deliverables
4. **Examples** - Input/output pairs, edge cases
5. **Validation** - Quality checklist

**Argument Syntax:** Use `{{variable_name}}` in body; define in `arguments` array

## Writing Rules

**Specificity:** Measurable criteria, explicit actions, defined terms
**Clarity:** Numbered steps (sequences), bullets (items), headings (concerns)
**Reusability:** Placeholders, documented substitutions, separated content
**Precision:** Examples for patterns, explicit format, edge case handling

## Patterns

**Code Gen:**

```markdown
# Generate <Type>

## Requirements

- Language: <lang>
- Framework: <fw>
- Functions: <list>

## Output Format

<structure>
## Constraints
<limits>
```

**Analysis:**

```markdown
# Analyze <Subject>

## Input

<format>
## Focus
1. <aspect>
## Output
<findings format>
```

**Transform:**

```markdown
# Transform <A> to <B>

## Source/Target

<formats>
## Rules
- <mapping>
```

## Anti-Patterns

❌ Vague goals | ❌ Implicit context | ❌ Missing format | ❌ Over-complexity | ❌ No examples | ❌ Buried requirements

## Checklist

- [ ] Metadata: AI provenance (if generated) + Copilot `name` + `description`/`author`/`tags`
- [ ] Arguments: Defined in frontmatter, used as `{{variable}}` in body
- [ ] Objective: Clear in first 2 sentences
- [ ] Success criteria: Measurable
- [ ] Format: Explicit or obvious
- [ ] Examples: Provided for complexity
- [ ] Edge cases: Addressed
- [ ] Testable: As written

**Token Target:** 300-800 typical | 1500 max complex

**Optimization:** Front-load critical info | Structure > prose | No filler ("please", "try to") | Examples > descriptions

---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-project-process-prompts"
prompt: |
  Create a prompt to create an Project Overview instruction file containing
  the high level context for the project. The instruction file should be
  optimized for AI agents and to minimize the token requirements.
started: "2025-12-28T00:00:00Z"
ended: "2025-12-28T00:10:00Z"
task_durations:
  - task: "design prompt structure"
    duration: "00:05:00"
  - task: "write prompt content"
    duration: "00:05:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-project-process-prompts/conversation.md"
source: "johnmillerATcodemag-com"
description: "Generate token-optimized project overview instruction file"
context: "Repository-wide instruction for AI agents, high-level project context"
expected_output: ".instructions.md file with project overview optimized for minimal tokens"
tools: ["search", "read", "create"]
mode: agent
name: create-project-overview
author: John Miller
tags: [instructions, project-overview, documentation, token-optimization]
arguments:
  - name: project_name
    description: Name of the project
  - name: project_type
    description: Type/category of project (e.g., web app, library, research)
  - name: primary_language
    description: Primary programming language(s)
  - name: key_technologies
    description: Core technologies, frameworks, or platforms
  - name: project_purpose
    description: High-level purpose or mission of the project
---

# Generate Project Overview Instruction File

Create a token-optimized `.instructions.md` file providing high-level project context for AI agents.

## Input

- **Project Name**: {{project_name}}
- **Project Type**: {{project_type}}
- **Primary Language(s)**: {{primary_language}}
- **Key Technologies**: {{key_technologies}}
- **Project Purpose**: {{project_purpose}}

## Required Sections

### 1. Project Identity

- Name and one-line purpose
- Type/category
- Primary language(s) and version(s)

### 2. Core Architecture

- Tech stack (frameworks, platforms, key libraries)
- High-level structure (monorepo, microservices, layers, etc.)
- Key directories and their purposes

### 3. Development Context

- Target environment(s) (web, mobile, desktop, cloud)
- Build/deployment model
- Key dependencies or external services

### 4. Code Standards

- Language-specific conventions
- Naming patterns
- File organization principles
- Testing approach (frameworks, coverage expectations)

### 5. Critical Constraints

- Performance requirements
- Security considerations
- Compatibility requirements
- Known limitations

## Output Requirements

**File**: `project-overview.instructions.md` in `.github/instructions/`

**Metadata**:

- Complete AI provenance (if AI-generated)
- `description`: One-line project summary
- `applyTo: "**"` (repository-wide)

**Content Style**:

- Imperative, directive tone
- Bullet/list format preferred
- No redundant explanations
- Specific over general
- Token-optimized (eliminate filler)

**Optimization Rules**:

- Use abbreviations where unambiguous
- Prefer lists over prose
- Omit obvious context
- State facts, not descriptions
- Group related items

## Example Structure

```markdown
---
description: "[Project name] - [one-line purpose]"
applyTo: "**"
[...metadata...]
---

# Project: [Name]

**Purpose**: [1-2 sentence mission]
**Type**: [category]
**Stack**: [lang] + [frameworks]

## Architecture

- **Structure**: [organization pattern]
- **Core Dirs**: `dir/` – [purpose]; `dir2/` – [purpose]
- **Build**: [tool/process]

## Standards

- [Language]: [version], [key conventions]
- Naming: [pattern]
- Tests: [framework], [coverage target]

## Environment

- Target: [platforms]
- Deploy: [method]
- Dependencies: [critical services]

## Constraints

- [constraint type]: [requirement]
```

## Validation Checklist

- [ ] All required sections present
- [ ] Metadata complete with `description` and `applyTo: "**"`
- [ ] Token-optimized (no filler words)
- [ ] Specific technologies/versions stated
- [ ] Directory structure documented
- [ ] Testing approach defined
- [ ] Critical constraints listed
- [ ] Imperative tone throughout

## Anti-Patterns to Avoid

❌ Verbose explanations of obvious concepts
❌ Redundant section introductions
❌ Vague technology references ("modern framework")
❌ Missing version information
❌ Prose format instead of lists
❌ Including implementation details (save for specific instruction files)
❌ Omitting key dependencies or constraints

## Success Criteria

1. AI agent can understand project scope in <200 tokens
2. Contains all information needed for initial orientation
3. Links to more detailed instruction files where appropriate
4. No ambiguous or vague statements
5. Every sentence provides unique, essential information

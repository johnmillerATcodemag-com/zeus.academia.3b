---
ai_generated: true
model: "openai/gpt-5.3-codex@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-25-custom-agents-instructions-generation"
prompt: |
  submit this prompt #file:create-custom-agents-instructions.prompt.md with these arguments:

  instruction_filename: custom-agents.instructions.md
  apply_to: .github/agents/**/*.agent.md)
  agent_scope: repository
  include_ide_notes: true
started: "2026-02-25T00:20:00Z"
ended: "2026-02-25T01:00:00Z"
task_durations:
  - task: "analyze prompt and docs requirements"
    duration: "00:04:00"
  - task: "generate instruction content"
    duration: "00:08:00"
  - task: "cross-reference and provenance updates"
    duration: "00:03:00"
  - task: "add persona guidance and behavior testing sections"
    duration: "00:25:00"
total_duration: "00:40:00"
ai_log: "ai-logs/2026/02/25/2026-02-25-custom-agents-instructions-generation/conversation.md"
source: ".github/prompts/create-custom-agents-instructions.prompt.md"
description: "GitHub Copilot custom-agent creation and maintenance standards"
applyTo: ".github/agents/**/*.agent.md"
---

# GitHub Copilot Custom Agents Standards

## Purpose and Scope

- Define how to create and maintain custom agent profiles for repository scope.
- Apply to repository-level agent files under `.github/agents/*.agent.md`.
- Support use in GitHub.com coding agent, supported IDEs, and Copilot CLI.

## File Placement and Naming Rules

- Repository-level agents: `.github/agents/<agent-name>.agent.md`.
- Organization/enterprise-level agents: `/agents/<agent-name>.agent.md` in a `.github-private` repository.
- Use unique, descriptive filenames.
- Allowed filename characters: `.`, `-`, `_`, `a-z`, `A-Z`, `0-9`.

## Agent Profile Structure

Agent profiles are Markdown files with YAML frontmatter plus a Markdown prompt body.

| Property                   | Required     | Notes                                                                                          |
| -------------------------- | ------------ | ---------------------------------------------------------------------------------------------- |
| `name`                     | No           | Display name. Defaults to filename if omitted.                                                 |
| `description`              | Yes          | Clear purpose and capabilities.                                                                |
| `target`                   | No           | `vscode` or `github-copilot`; omit for both.                                                   |
| `tools`                    | No           | Omit or `[*]` = all, `[]` = none, list = explicit set.                                         |
| `disable-model-invocation` | No           | When true, prevents automatic model invocation; manual selection required.                     |
| `user-invocable`           | No           | When false, cannot be manually selected by user.                                               |
| `infer`                    | No (retired) | Deprecated. Prefer `disable-model-invocation` and `user-invocable`.                            |
| `mcp-servers`              | No           | Extra MCP config in profile; not used in VS Code and other IDE custom agents for coding agent. |
| `metadata`                 | No           | Name/value annotations; not used in VS Code and other IDE custom agents for coding agent.      |

### IDE Notes

- IDE custom agents may use `model`.
- IDE custom agents may include `argument-hint` and `handoffs` depending on IDE support.
- For GitHub.com coding agent, `model`, `argument-hint`, and `handoffs` are ignored for compatibility.

## Tools and MCP Guidance

- Use least-privilege tool sets for each agent purpose.
- Common aliases:
  - `read` for file reads
  - `search` for file/text search
  - `edit` for modifications
  - `execute` for shell usage
  - `agent` for custom-agent delegation
- Namespaced tools:
  - Specific tool: `<server>/<tool>`
  - All server tools: `<server>/*`
- Tool behavior:
  - Omit `tools` => all available tools enabled.
  - `tools: []` => no tools enabled.
  - `tools: [..]` => only listed tools enabled.
- For `mcp-servers`, configure only what the agent needs and keep tool scope explicit.
- Secrets and environment variables for MCP must be configured in Copilot repository environment settings.

## Prompt Authoring Guidance

- Define role and scope in first paragraph.
- List explicit responsibilities and hard boundaries.
- Specify output format and acceptance criteria.
- Define tone and communication style requirements (concise, structured, evidence-based, etc.).
- Include constraints for safety and repo conventions.
- Keep instructions concrete and testable.

### Anti-Patterns

- Overly broad prompts without domain boundaries.
- Unrestricted shell execution when unnecessary.
- Missing output constraints (format, scope, files).
- Implied behavior that depends on hidden assumptions.
- Referencing local editor tasks or helper files that are not committed in the repository.

### Runtime-Readiness Guardrails for Agents

Repository agents MUST include runtime-readiness checks in their default workflow whenever the slice adds routes, validation, or persistence.

- Check the application host for endpoint registration before declaring a feature complete.
- Check the feature-local persistence plan for migration artifacts or explicit migration ownership.
- Confirm validation is performed on the normalized domain value when canonicalization occurs.
- Confirm invalid input is mapped to a client-safe validation result instead of a raw 500 response.
- Require evidence (commands, files changed, or test output) before marking a slice as complete.

If a slice is not runtime-ready, the agent must report that as a blocker, not as an assumed success condition.

### Persona Definition (required for role/persona-type agents)

When the agent represents a professional role, include all of the following in the prompt body:

| Element                 | Definition                                                                                                                                                                                                                                                                                    | Example                                                                                                        |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| **Skills**              | Concrete capabilities the agent _can do_, each with a proficiency level: `basic`, `intermediate`, or `advanced`                                                                                                                                                                               | Backlog grooming (advanced), KPI definition (intermediate)                                                     |
| **Actions**             | Observable behaviors the agent _will do_ as its default workflow. Classify each as **Simple** (fully defined inline in the `.agent.md` prompt body) or **Complex** (defined in a separate prompt file — note existing path or intended path + one-line description if it needs to be created) | Simple: "asks clarifying questions first"; Complex: "generates PRD" → `.github/prompts/generate-prd.prompt.md` |
| **Expertise**           | Domain authority and depth the agent _embodies_ — what it knows, how well, and in which subdomain                                                                                                                                                                                             | Senior PM in education SaaS; advanced in prioritization frameworks; intermediate in technical architecture     |
| **Escalation triggers** | Explicit list of what is _out of scope_ — when to decline, defer to a human, or hand off                                                                                                                                                                                                      | Do not produce database schema changes; escalate legal/compliance questions                                    |
| **Evidence standards**  | Required inputs before any recommendation                                                                                                                                                                                                                                                     | Do not propose priorities without impact/effort data; do not claim stakeholder approval that was not provided  |

## Persona Guidance

Use the following template when defining a persona-type agent. Populate all rows; omit none.

```markdown
### Skills

| Skill        | Proficiency |
| ------------ | ----------- | ------------ | -------- |
| <skill name> | basic       | intermediate | advanced |

### Actions

| Action               | Type              | Prompt File (if complex)                                |
| -------------------- | ----------------- | ------------------------------------------------------- |
| <action description> | Simple \| Complex | <path or "needs to be created: <path> — <description>"> |

### Expertise

<One paragraph: domain, depth, known subdomain strengths and limits>

### Escalation Triggers

- <Condition 1: when to decline or defer>
- <Condition 2: when to hand off to another agent>

### Evidence Standards

- <Signal or input required before making recommendation 1>
- <Signal or input required before making recommendation 2>
```

## Agent Behavior Testing

For each agent, include at least two representative test prompts with expected behaviors:

```markdown
**Test 1 — Core behavior**
Prompt: "<prompt text>"
Expected: <summary of what the agent should do or produce>

**Test 2 — Boundary/refusal**
Prompt: "<prompt that should be declined or escalated>"
Expected: Agent declines/escalates and states the reason
```

- Verify the agent uses its defined output structure.
- Confirm escalation triggers fire correctly on out-of-scope inputs.
- Check that evidence standards prevent unsupported recommendations.

## Processing and Precedence Rules

- Name conflicts resolve by lowest level precedence:
  - Repository overrides organization
  - Organization overrides enterprise
- Agent versioning follows Git commit SHAs for the profile file.
- `tools` acts as a filter on built-in and MCP tools according to value.

## Environment Differences

- GitHub.com coding agent supports core profile properties and processes agents during issue/task/PR workflows.
- IDEs (VS Code, JetBrains, Eclipse, Xcode) support profile authoring from in-editor flows and may expose additional properties.
- Some properties are environment-specific or ignored outside their intended runtime.

## Operational Safety

- Default to least-privilege `tools`.
- Restrict `execute` unless the role clearly requires shell operations.
- Never hardcode credentials in prompts or `mcp-servers` env blocks.
- Keep MCP configuration narrowly scoped to required tools.
- Review prompt boundaries to prevent unintended file or command scope.

## Validation Checklist

- [ ] `description` exists and clearly defines purpose.
- [ ] `tools` scope matches least-privilege intent.
- [ ] Deprecated `infer` is not used for new profiles.
- [ ] `disable-model-invocation` and `user-invocable` are set intentionally when needed.
- [ ] Any `mcp-servers` entries map to real server/tool names.
- [ ] Secrets/env references use approved Copilot environment configuration.
- [ ] Prompt body includes role, scope, constraints, and output expectations.
- [ ] Tone and communication style requirements are stated.
- [ ] Environment-specific properties are documented.
- [ ] Prompt body stays within platform size limits.
- [ ] **Persona agents**: skills defined with proficiency levels.
- [ ] **Persona agents**: each action classified as simple (inline) or complex (prompt file).
- [ ] **Persona agents**: complex actions reference an existing prompt file path or specify an intended path and description.
- [ ] **Persona agents**: escalation triggers explicitly stated.
- [ ] **Persona agents**: evidence standards defined for all recommendation behaviors.
- [ ] At least two behavior test prompts included (one core, one boundary/refusal).
- [ ] Any command, script path, or task reference in the prompt body is executable from repository contents as committed.
- [ ] If a task reference is used, the corresponding task definition file exists in-repo; otherwise a canonical script command is provided.

## Example Profiles

### Minimal Profile

```markdown
---
name: readme-creator
description: Creates and improves README documentation only
---

You are a documentation specialist focused on README files only.
Do not modify source code files.
```

### Scoped Profile

```markdown
---
name: implementation-planner
description: Produces implementation plans and technical specs
tools: ["read", "search", "edit"]
mcp-servers:
  github-docs:
    type: local
    command: docs-mcp
    tools: ["search"]
---

Create structured implementation plans with risks, dependencies, and acceptance criteria.
Do not implement production code unless explicitly requested.
```

### Persona Profile

```markdown
---
name: product-manager
description: Product Manager persona focused on requirements, prioritization, and delivery alignment
tools: ["read", "search", "edit"]
---

The universe of discourse is Academia Management.

You are a senior Product Manager.

#### Skills

| Skill                            | Proficiency  |
| -------------------------------- | ------------ |
| Requirements definition          | advanced     |
| Backlog grooming                 | advanced     |
| Prioritization (RICE, MoSCoW)    | advanced     |
| Stakeholder communication        | intermediate |
| Technical feasibility assessment | intermediate |

#### Actions

| Action                                                   | Type    | Prompt File                                                    |
| -------------------------------------------------------- | ------- | -------------------------------------------------------------- |
| Clarify goals and constraints before proposing solutions | Simple  | —                                                              |
| Produce structured PRDs                                  | Complex | `.github/prompts/generate-prd.prompt.md` (needs to be created) |
| Present prioritization rationale                         | Simple  | —                                                              |
| Define acceptance criteria before handoff                | Simple  | —                                                              |

#### Expertise

Senior PM in education and academic SaaS. Advanced in roadmap planning and prioritization frameworks. Intermediate in backend architecture.

#### Escalation Triggers

- Do not approve or reject architectural decisions — escalate to tech lead.
- Do not produce legal, compliance, or security rulings.

#### Evidence Standards

- Do not propose priorities without impact/effort estimates.
- Do not claim stakeholder approval that was not provided.
- State assumptions explicitly when data is missing.

#### Behavior Tests

**Test 1 — Core behavior**
Prompt: "Draft requirements for a student grade export feature."
Expected: Structured output with problem, goals, non-goals, success metrics, and open questions.

**Test 2 — Boundary/refusal**
Prompt: "Approve this database schema change."
Expected: Agent declines, states this is an architectural decision, and suggests escalating to the tech lead.
```

## Maintenance

- Re-validate this instruction when GitHub updates custom-agent docs.
- Keep examples aligned with currently supported properties.
- Remove or mark retired properties when guidance changes.

After creating this instruction file, update `.github/instructions/project-overview.instructions.md` to add a reference to this custom-agents instruction in the Standards or Development Process section.

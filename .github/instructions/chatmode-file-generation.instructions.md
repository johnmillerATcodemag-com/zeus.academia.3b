---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-chatmode-guide"
prompt: |
  create an token optimized instruction file to guide an ai assistant in the generation of chatmode files.
started: "2025-10-28T16:00:00Z"
ended: "2025-10-28T16:06:00Z"
task_durations:
  - task: "design"
    duration: "00:03:00"
  - task: "draft"
    duration: "00:03:00"
total_duration: "00:06:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-chatmode-guide/conversation.md"
source: "johnmillerATcodemag-com"
description: "Token-optimized guide for generating chatmode files"
applyTo: "**/*.chatmode.md"
---

# Chatmode File Generation

## Naming & Location

`<chatmode-purpose>.chatmode.md` in `.github/chatmodes/`

## Metadata Requirements

### AI Provenance (AI-generated files only)

```yaml
ai_generated: true
model: "<provider>/<model>@<version>"
operator: "<github-username>"
chat_id: "<chat-id>"
prompt: |
  <exact request>
started: "<ISO8601>"
ended: "<ISO8601>"
task_durations:
  - task: "<name>"
    duration: "<hh:mm:ss>"
total_duration: "<hh:mm:ss>"
ai_log: "ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/conversation.md"
source: "<creator>"
```

### Copilot chatmode Metadata

**Required:** `name`, `description`
**Recommended:** `chatmode_type`, `capabilities: [...]`, `tools: [...]`, `temperature`
**Optional:** `instructions`, `icon`, `examples`, `author`, `version`

**Types:** `coding` | `research` | `testing` | `documentation` | `deployment` | `monitoring` | `data-analysis` | `review` | `planning`

```yaml
name: "network-architect"
description: "Assists with VLAN segmentation, firewall rules, device onboarding"
chatmode_type: "coding"
capabilities: ["network-config", "vlan-design", "firewall-rules"]
tools: [web, terminal]
temperature: 0.3
instructions: |
  Provide modular, step-by-step networking guidance
icon: "🌐"
examples:
  - user: "How do I configure IoT VLAN?"
    chatmode: "Validated checklist for IoT VLAN setup..."
```

## Content Structure

1. **Overview** - Name + primary objective (1-2 sentences)
2. **Capabilities** - Action-oriented bullets ("Generates X", "Analyzes Y")
3. **Triggers** - Activation conditions (commands, files, schedules, events)
4. **Configuration** - Parameters with type/required/default/description
5. **Workflow** - Numbered execution steps (input → process → output → errors)
6. **Examples** - Scenario/Input/Output format
7. **Constraints** - Limits, dependencies, requirements

**Parameter Format:**

```yaml
param_name:
  type: string|number|boolean|array|object
  required: true|false
  default: <value>
  description: <brief>
```

## Type-Specific Requirements

**Coding:** Languages, frameworks, tests, quality standards, file patterns
**Research:** Search strategies, sources, credibility criteria, output format
**Testing:** Test types, coverage thresholds, reporting format
**Deployment:** Environments, prerequisite checks, rollback procedures

## Optimization Rules

**Eliminate:** Generic descriptions | Obvious capabilities | Redundant examples
**Compress:** Tables for multi-attribute parameters | Combined capabilities | Reference docs vs. duplicate
**Prioritize:** Most-used capabilities first | Common triggers before edge cases | Required before optional

## Checklist

- [ ] Metadata: AI provenance (if generated) + `name`/`description`/`chatmode_type`
- [ ] Capabilities: ≥3 actionable, specific items
- [ ] Triggers: Clearly defined, testable
- [ ] Config: All required parameters documented
- [ ] Workflow: Happy path + error handling
- [ ] Examples: Non-trivial use cases
- [ ] No conflicts in constraints/capabilities

## Integration

Reference related files with relative paths from repo root:

- **Instructions:** `.instructions.md` files chatmode follows
- **Prompts:** `.prompt.md` templates chatmode uses
- **Chatmodes:** Dependencies or handoffs

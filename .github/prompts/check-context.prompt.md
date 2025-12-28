---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-10-28-context-validation-v2"
prompt: |
  update #file:check-context.prompt.md. create an token optimized prompt file to guide an ai assistant in validating the current context.
started: "2025-10-28T16:25:00Z"
ended: "2025-10-28T16:28:00Z"
task_durations:
  - task: "review and validate"
    duration: "00:03:00"
total_duration: "00:03:00"
ai_log: "ai-logs/2025/10/28/2025-10-28-context-validation-v2/conversation.md"
source: "johnmillerATcodemag-com"
description: "Validate context for conflicts and inconsistencies"
context: "Instruction files, prompts, agents, and workspace context"
expected_output: "Structured conflict analysis with recommendations"
tools: ["search", "edit", "fetch"]
mode: agent
---

# Context Validation

Analyze current context (instructions, prompts, agents, workspace) for conflicts, contradictions, and inconsistencies.

## Validation Categories

1. **Conflicting Instructions**: Contradictory directives, incompatible requirements
2. **Factual Inconsistencies**: Contradictory data, dates, numbers, or specifications
3. **Logical Contradictions**: Mutually exclusive statements, circular dependencies
4. **Priority Conflicts**: Competing priorities, incompatible scopes
5. **Technical Incompatibilities**: Conflicting tech stacks, version/dependency conflicts
6. **Terminology Gaps**: Inconsistent naming, undefined references, missing connections
7. **Redundancy**: Duplicate instructions, overlapping processes

## Output Format

For each issue found:

```yaml
category: <conflict type>
description: <what conflicts>
location: <where it occurs>
impact: <severity: critical|high|medium|low>
recommendation: <resolution approach>
```

If no conflicts: state "Context validated. No conflicts detected."

## Validation Steps

1. Load all instruction, prompt, and agent files
2. Cross-reference directives for contradictions
3. Check terminology consistency across files
4. Identify duplicate or overlapping content
5. Verify technical compatibility
6. Report findings using output format

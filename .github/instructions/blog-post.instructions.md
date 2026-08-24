---
ai_generated: false
applyTo: "jekyll-src/_posts/*.blog.md, CODE/*.blog.md"
---

# Blog Post Writing Instructions

This instruction file defines standards and patterns for authoring blog posts in the AI Practitioner blog and CODE Magazine. These guidelines are derived from recent post examples (AIAGSD4, AIAGSD5, AIAGSD6) and the CODE-STYLE-GUIDE, emphasizing implementation-focused, technically rigorous writing that serves both blog and publication audiences.

## Table of Contents

1. [YAML Front Matter](#yaml-front-matter)
2. [Post Structure](#post-structure)
3. [Narrative Voice & Tone](#narrative-voice--tone)
4. [Section Design Rules](#section-design-rules)
5. [Prompt Documentation Pattern](#prompt-documentation-pattern)
6. [Artifact Summary Pattern](#artifact-summary-pattern)
7. [List and Table Conventions](#list-and-table-conventions)
8. [Linking Conventions](#linking-conventions)
9. [Code Examples and Formatting](#code-examples-and-formatting)
10. [Closing Sections](#closing-sections)
11. [Quality Checklist](#quality-checklist)

---

## YAML Front Matter

Every blog post **must** include YAML front matter with these required fields:

```yaml
---
layout: post
title: "[Title]"
date: YYYY-MM-DD
categories: [category1, category2] # or single string "category"
excerpt: "[Brief summary for feed/preview, 1 line]"
description: "[Longer description for meta/search, 1-2 sentences]"
image: /assets/images/YYYY-MM-DD/[filename]
---
```

### Field Guidelines

- **layout:** Always `post` (Jekyll standard)
- **title:** Action-oriented, descriptive, include series/part number if applicable
  - ✅ "AI-Assisted Greenfield Software Development, Part 6: Vertical Slices and Implementation Planning"
  - ✅ "Welcome to the AI Practitioner's Blog"
  - ❌ "Part 6" (too vague)
- **date:** ISO 8601 format (YYYY-MM-DD). For blog publication, use the intended publish date
- **categories:** Either a string `"category"` or array `[cat1, cat2]`. Use 1-3 categories focusing on primary topic
  - Examples: `[ai-assisted-development, software-engineering, architecture]` or `ai-assisted-development`
- **tags:** (Optional) Array of specific keywords for discoverability; 4-6 tags recommended
  - Distinct from categories; more granular
- **excerpt:** One-line elevator pitch for social feeds and listing pages
- **description:** 1-2 sentences for meta description; impacts SEO. Should answer "what will I learn?"
- **image:** Path to header image in `/assets/images/YYYY-MM-DD/` folder. Always include (builds trust, improves engagement)

### Example

```yaml
---
layout: post
title: "AI-Assisted Greenfield Software Development, Part 5: Technology Guardrails"
date: 2026-02-24
categories: [ai-assisted-development, software-engineering, architecture, cqrs]
tags: [cqrs, instruction-files, guardrails, technology-standards]
excerpt: "Learn how to create technology-specific instruction files that guide AI agents in generating consistent, compliant code."
description: "Part 5 of the AIAGSD series defines technology instruction files and patterns that ensure AI-generated code meets language, framework, and architectural standards."
image: /assets/images/2026-02-24/AIAGSD5_Header_Large.png
---
```

---

## Post Structure

Use this baseline structure for all blog posts:

Blog post files **must** use the `.blog.md` extension. Store Jekyll posts in `jekyll-src/_posts/` and CODE Magazine versions in `CODE/`; do not create blog posts with a plain `.md` extension.

1. **YAML front matter** (as above)
2. **TL;DR or lead paragraph** (1-2 sentences establishing context and value)
3. **Series context introduction** (if part of a series; links to prior parts)
4. **<!--more--> comment** (Jekyll pagination marker)
5. **Header figure** (image + figcaption)
6. **Problem framing section** (`## Why X Matters`; SEO-friendly)
7. **Body content** (major sections with H2, subsections with H3)
8. **Closing sections** (What's Next, Feedback Loop, Disclaimer, Prompts)

### Rule: No H2 Immediately Followed by H3

An H2 heading must always have body copy before an H3. Never structure:

```markdown
## Major Section

### Subsection
```

Always insert content:

```markdown
## Major Section

Introductory copy explaining the section.

### Subsection
```

---

## Narrative Voice & Tone

- **First person singular** for execution narrative: "I ran", "I submitted", "I executed", "I created"
- **First person plural** for team/shared outcomes: "we can now", "we've established"
- **Implementation-focused**, not promotional: Explain how things work, not why they're "amazing"
- **Explain why before how:** When introducing a process or decision, state the reasoning first
  - ✅ "Without explicit architectural rules, AI agents tend to optimize for convenience rather than coherence. Architecture instruction files solve this by providing a clear architectural pattern, implementation rules, and anti-patterns to avoid."
  - ❌ "Let me show you architecture instruction files for implementing CQRS."
- **Professional, accessible tone:** Clear language; avoid unnecessary jargon; define acronyms on first use
- **Follow AP Style Guide:** Sentence structure, headlines, lists, punctuation conventions
- **Minimize passive voice:** "The prompt generates a file" not "A file is generated by the prompt"
- **Capitalize technology names correctly:** Microsoft Copilot (not CoPilot), GitHub (not Github), ChatGPT, Azure, ASP.NET Core, Vue 3

### Anti-Patterns to Avoid

- Overuse of marketing language ("revolutionary", "game-changing", "incredible")
- Passive constructions that hide agency ("it can be used to...")
- Hedging that undermines confidence ("might", "could potentially")
- Redundant explanations of the same concept
- Assuming reader knowledge of niche topics without context

---

## Section Design Rules

### Heading Hierarchy

- **H2** (`##`) for major phases, processes, or artifacts
  - Action-oriented and SEO-friendly
  - Examples: "Creating the Custom-Agents Instruction Prompt", "Generating the Product-Manager Agent File", "Why Architecture Instruction Files Matter"
- **H3** (`###`) for concrete sub-topics, artifact details, or process steps
  - Examples: "What the generated `custom-agents.instructions.md` file adds", "Reading the Slice Dependency Diagram"

### Section Titles Best Practices

Titles should be:

- **Action-oriented** when describing a process: "Creating...", "Generating...", "Implementing..."
- **Interrogative** for contextual or explanatory sections: "Why X Matters", "What Is CQRS?", "When to Use..."
- **Explicit and SEO-friendly**: Include key terms; avoid vague language
  - ✅ "Generating the Custom-Agents Instruction File"
  - ❌ "The Next Step"

### Content Before Subheadings

Always include body copy between H2 and H3:

```markdown
## Major Topic

[1-2 paragraphs of context/explanation]

### Subtopic 1

[Content]
```

---

## Prompt Documentation Pattern

When documenting a prompt execution, use this rhythm:

1. **Setup paragraph** (1-2 sentences): Explain the intent and why you're running this prompt
2. **Fenced code block** with the complete prompt text (publish as-is, verbatim)
3. **Short bullet list** (3-5 bullets): "What this prompt produces"
   - Focus on concrete outputs and deliverables
   - Use parallel phrasing across bullets
4. **Impact paragraph** (2-3 sentences): Connect the output to architecture, workflow, or project impact
   - Explain how this artifact improves quality, consistency, or predictability

### Example

```markdown
To build a reusable prompt for agent-governance instructions, I submitted this prompt:

\`\`\`text
using https://docs.github.com/en/copilot/reference/custom-agents-configuration,
https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-custom-agents,
https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents
create a prompt that will create an instruction file for creating agents.
\`\`\`

What this prompt produces:

- A parameterized prompt file designed to generate token-optimized `.instructions.md` content
- Input controls for `instruction_filename`, `apply_to`, `agent_scope`, and `include_ide_notes`
- A repeatable workflow for creating agent-governance guidance across repositories

Architecture/workflow impact: this creates a stable generation pattern for agent standards,
which improves consistency across slices and reduces prompt drift.
```

### Linking Prompts and Generated Artifacts

When a prompt generates a file, link to the generated artifact:

```markdown
[custom-agents.instructions.md](https://github.com/JohnMichaelMiller/zeus.academia.3b/blob/Part-Six/.github/instructions/custom-agents.instructions.md)

This is the execution step where the reusable prompt is run with concrete project settings...
```

---

## Artifact Summary Pattern

When describing generated instruction files or major artifacts, follow the **thesis-first paragraph pattern**:

1. **Thesis sentence** (1 strong sentence): State what the artifact is and its core purpose
   - ✅ "The generated instruction file becomes the operational contract for anyone creating `.agent.md` files in the repo, encoding enforced patterns that keep planning agents consistent and safe across vertical slices."
   - ❌ "The instruction file has several parts..." (factual but no thesis)

2. **First supporting paragraph** (4-6 sentences): Describe enforced patterns, governance rules, and integration points
   - What does this artifact enforce?
   - What are the key compliance mechanisms?
   - How does it integrate with other systems?

3. **Second supporting paragraph** (3-5 sentences): Anti-patterns, consequences, and downstream impact
   - What mistakes does this prevent?
   - What happens if you violate these rules?
   - How does following this improve project outcomes?

### Example

```markdown
### What the generated `custom-agents.instructions.md` file adds

The generated instruction file becomes the operational contract for anyone creating `.agent.md`
files in the repo, encoding enforced patterns that keep planning agents consistent and safe
across vertical slices.

The file is structured around two core enforced patterns. First, **governance and boundaries**:
it defines placement rules (`.github/agents/<name>.agent.md`), required YAML fields
(`description`, `tools`, `target`, `disable-model-invocation`, `user-invocable`, `mcp-servers`,
`metadata`), and least-privilege tool defaults that prevent agents from accessing more authority
than their scope requires. Second, **persona integrity**: it specifies how planning agents must
declare their skills, escalation triggers (political, security, architectural), and evidence
standards so authors, reviewers, and users understand agent limitations before deployment.

The anti-patterns section prevents common drift: agents that claim broad competence without
explicit scope notes, agents that attempt to execute code or modify systems without governance
approval, profiles missing behavior-test examples, and environment assumptions that don't
distinguish GitHub.com from IDE behavior. Integration points include direct references to prior
instruction files (so agents follow repository-wide standards), validation checklists that must
pass before merge, and example profiles (minimal, scoped persona) that accelerate adoption for
new contributors.
```

---

## List and Table Conventions

- **Bullet lists** (`-` or `*`): Use for capabilities, features, outputs, or non-sequential items
  - Keep bullets parallel in phrasing and grammar
  - Example:

    ```markdown
    What this prompt produces:

    - A parameterized prompt file designed to generate token-optimized `.instructions.md` content
    - Input controls for `instruction_filename`, `apply_to`, `agent_scope`, and `include_ide_notes`
    - A repeatable workflow for creating agent-governance guidance across repositories
    ```

- **Numbered lists** (`1.`, `2.`): Use for sequential processes, steps, or priorities
  - Example:

    ```markdown
    Validation checks before implementation:

    1. Does the PM agent govern its own scope?
    2. Does the PRD structure enforce clarity without over-specification?
    3. Can implementation instructions reference both governance and PRD artifacts?
    ```

- **Bold lead-ins in bullets**: Use when category labels improve scanability
  - Example:
    ```markdown
    - **Governance and boundaries**: Defines placement rules, required YAML fields, and least-privilege defaults
    - **Persona integrity**: Specifies how agents must declare skills and escalation triggers
    ```

- **Tables** (Markdown): Use compact two-column layouts for file inventories and quick reference
  - Keep content concise; link to detailed explanations in body text
  - Example:
    ```markdown
    | File                                          | Purpose                                                                   |
    | --------------------------------------------- | ------------------------------------------------------------------------- |
    | `create-custom-agents-instructions.prompt.md` | Generates custom-agent instruction-file prompts from official GitHub docs |
    | `custom-agents.instructions.md`               | Defines repository standards for authoring `.agent.md` profiles           |
    ```

---

## Linking Conventions

- **Series links**: Include near the top of the post for context
  - Use clear series identifiers ("Part 1", "Part 2", etc.)
  - Example: "[See earlier posts in the AIAGSD series for context.](https://blog.pdata.com/AIAGSD1/)"

- **Repository artifacts**: Link by exact filename when possible
  - Example: "[custom-agents.instructions.md](https://github.com/...)"

- **External resources**: Link to documentation, standards, and source materials
  - Purpose-driven; avoid redundant links within a short span
  - Always use the official capitalization and URL structure (Microsoft Copilot docs, GitHub docs, etc.)

- **Email links**: In Feedback Loop section, use mailto links
  - Example: `Send your thoughts to **john.miller@codemag.com**`

---

## Code Examples and Formatting

- **Inline code**: Use backticks for variable names, filenames, field names, command names
  - ✅ `custom-agents.instructions.md`, `@tool`, `--help`
  - Use filenames in links when possible rather than inline code

- **Code blocks**: Use fenced blocks with language identifier
  - For prompts: Use `\`\`\`text` or bare fences
  - For code samples: Use language identifiers (`\`\`\`csharp`, `\`\`\`json`, `\`\`\`yaml`)
  - Always publish **full, verbatim** prompt text (including verbose iterations)

- **YAML/JSON examples**: Use proper indentation and syntax highlighting

  ```yaml
  ---
  ai_generated: true
  model: "anthropic/claude-3.5-sonnet@2024-10-22"
  operator: "john-miller"
  ---
  ```

- **Emphasis**: Use italic (`_text_`) for new concepts, bold (`**text**`) for key terms, em-dashes for clarity
  - Example: "I'll shift from _architecture-level_ instructions to _technology-level_ instructions."

---

## Closing Sections

Every post **must** end with these three sections plus a Prompts log:

### 1. What's Next?

Provide a concrete preview of the next article, post, or phase:

```markdown
## What's Next?

We'll finish this phase by creating implementation prompts that guide AI agents through
coding work while keeping generated output aligned with our technology-specific instruction
files and architectural vision. In the next step, I'll use the planning and persona-agent
outputs to define implementation slices with clear dependencies and acceptance criteria.
```

- **For series posts**: Reference the next part explicitly by number and topic
- **For standalone posts**: Reference related work or future directions
- Avoid vague endings ("stay tuned"); be specific about scope and deliverables

### 2. Feedback Loop

Provide a clear channel for reader feedback:

```markdown
## Feedback Loop

Feedback is always welcome. Send your thoughts to **john.miller@codemag.com**.
```

- Link email as `mailto:` for direct interaction
- Keep brief; one sentence recommended

### 3. Disclaimer

Statement about AI assistance and human review:

```markdown
## Disclaimer

AI contributed to the writing of this post, but humans reviewed it, refined it, enhanced it,
and gave it soul.
```

- Standardized phrasing across all posts (shows consistency and transparency)
- Always acknowledge human curation

### 4. Prompts:

List the prompt operations used to create or modify the blog post itself. These are authoring prompts, revision requests, and editorial instructions submitted to the writing agent. They are not the implementation prompts, source specifications, or other subject-matter artifacts being explained in the article unless those prompts were also used to create or modify the post.

```markdown
Prompts:

- [Description of what you asked AI to do with the prompt]
- [Another prompt operation, same format]
- [Include verbose iterations if they shaped the final output]
```

- Publish authoring prompts **exactly as written** (verbatim), including verbose or multi-turn iterations
- One bullet per distinct prompt execution
- Format: action verb + brief description
  - ✅ "Expand on this post with descriptions of this prompt: '...'"
  - ✅ "Refactor artifact summaries: Convert feature lists to thesis-first paragraphs"
  - ❌ "Added some content" (too vague)

---

## Quality Checklist

Before finalizing any blog post, verify:

### Structure & Navigation

- [ ] YAML front matter complete (layout, title, date, categories, excerpt, description, image)
- [ ] Series links present (if applicable) and point to correct posts
- [ ] <!--more--> comment present after intro
- [ ] Header image with figure/figcaption included
- [ ] No H2 immediately followed by H3 (body copy between heading levels)

### Content Quality

- [ ] TL;DR or intro establishes value and context
- [ ] Problem framing section present ("Why X Matters" or similar)
- [ ] Every major prompt appears in a fenced code block (verbatim)
- [ ] Output impact explained after each prompt (architecture/workflow impact)
- [ ] Artifact summaries follow thesis-first paragraph pattern
- [ ] At least one inventory table or structured list improves scanability
- [ ] All terminology consistent across post
- [ ] Parallel phrasing in sibling bullet lists

### Links & References

- [ ] Episode/part links accurate and current
- [ ] GitHub/artifact links functional (where applicable)
- [ ] External documentation links to official sources with correct capitalization
- [ ] Email links use `mailto:` format in Feedback section

### Narrative & Tone

- [ ] Written in first person (singular for personal narrative, plural for team outcomes)
- [ ] Explains _why_ before _how_ for major processes
- [ ] Minimizes passive voice
- [ ] Technology names follow official capitalization
- [ ] Acronyms spelled out on first use
- [ ] No marketing hype; implementation-focused throughout

### Closing Sections

- [ ] "What's Next?" section present with specific preview of next work
- [ ] "Feedback Loop" section with email contact
- [ ] "Disclaimer" section acknowledging human review
- [ ] "Prompts:" log listing all prompt operations (as-is, including iterations)

### Mechanics

- [ ] Spaced em-dashes in prose: `word — word` (not `word - word`)
- [ ] No accidental capitalization drift (e.g., "Vision" mid-sentence)
- [ ] Code block syntax highlighting correct (language identifiers present)
- [ ] Tables render properly in Markdown
- [ ] No broken references or dead links
- [ ] Spell-checked for typos and consistency

### For CODE Magazine Publication (if applicable)

- [ ] All `blog.pdata.com/` URLs replaced with `www.codemag.com/blog/AIPractitioner/`
- [ ] All `AIPractitioner@pdata.com` email addresses replaced with `john.miller@codemag.com`
- [ ] Original post retained in jekyll-src/\_posts/ directory
- [ ] Publication-ready version saved in CODE/ folder
- [ ] No content loss or structural changes between versions

---

## Examples from Recent Posts

**Well-structured post**: [AIAGSD6](jekyll-src/_posts/2026-03-01-AIAGSD6.blog.md)

- Governance checkpoint section validates artifact coherence
- Thesis-first artifact summaries
- Clear prompt documentation pattern
- Complete closing sections with Prompts log

**Architectural deep dive**: [AIAGSD5](jekyll-src/_posts/2026-02-24-AIAGSD5.blog.md)

- Problem framing ("Why Technology Instruction Files Matter")
- Meta-prompt workflow (creating prompts that generate prompts)
- Technology-specific guidance sections

**Series foundation post**: [AIAGSD4](jekyll-src/_posts/2026-01-26-AIAGSD4.blog.md)

- Pattern explanation with visual metaphors
- Step-by-step process documentation
- Clear anti-patterns section

---

## Appendix: Recent Post Checklist Audit

Posts scored against this instruction file's criteria:

| Post    | H2+Body    | Prompts | Artifacts         | Closing      | Narrative |
| ------- | ---------- | ------- | ----------------- | ------------ | --------- |
| AIAGSD6 | ✅         | ✅      | ✅ (thesis-first) | ✅           | ✅        |
| AIAGSD5 | ✅         | ✅      | ✅ (thesis-first) | ✅           | ✅        |
| AIAGSD4 | ✅         | ✅      | ✅ (list-based)   | ✅           | ✅        |
| WTAIP   | ⚠️ (brief) | —       | —                 | ⚠️ (partial) | ✅        |

**Legend**: ✅ = compliant, ⚠️ = partial, — = not applicable

These standards weight recent, comprehensive posts (AIAGSD4–6) most heavily while accommodating variations for shorter or standalone pieces.

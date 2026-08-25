---
ai_generated: true
model: "github/copilot@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "78bccef1-7df6-4b32-99b6-2cc4a743aecc"
prompt: |
  create a concise human readable description of the issues discovered in this chat and the changes made to address them. put this in the docs folder
started: "2026-08-24T16:50:00-07:00"
ended: "2026-08-24T16:53:00-07:00"
task_durations:
  - task: "condense issues and corrective changes"
    duration: "00:02:00"
  - task: "place and validate documentation"
    duration: "00:01:00"
total_duration: "00:03:00"
ai_log: "ai-logs/2026/08/24/78bccef1-7df6-4b32-99b6-2cc4a743aecc/conversation.md"
source: ".github/prompts/academia-implementation/application-host-and-persistence-composition-implementation.prompt.md"
description: "Human-readable summary of Academia architecture issues and corrective changes"
---

# Academia Architecture Issues and Changes

## Issues Discovered

The project faced several architectural issues. Firstly, there was no clearly defined application host for registering endpoints, managing dependency injection, handling SQL Server interactions, or executing migrations. Although migration requirements were identified, there was a lack of ownership assigned to specific features, DbContexts, or migration folders. Additionally, `ProvisionExtension` was incorrectly directed towards the Shared Kernel DbContext, despite the project utilizing feature-local persistence boundaries. Furthermore, there was confusion between `University.Name` and the canonical university catalog code, which is actually represented as `University_code` in the domain model. Lastly, the distinction between shared domain rules and feature-owned persistence was not clearly defined.

## Changes Made

The changes added a Phase 0 implementation prompt for Application Host and Persistence Composition and clarified that application hosting, endpoint registration, dependency injection, authentication, and migration execution are outside the Shared Kernel. They defined feature-local DbContexts with one migration owner per table, assigning `ProvisionExtensionDbContext` sole ownership of `Extensions` migrations while retaining the `Extension` entity and reusable mapping semantics in Shared Kernel. They also clarified that ManageUniversities uses a feature-local catalog keyed by `Code` rather than treating `University.Name` as the catalog code. Finally, the implementation prompts, repository instructions, execution maps, workflow plans, and refactoring plan were updated to enforce these decisions.

## Status

The planning work is complete. No application host or feature slice implementation has been added yet. The remaining implementation prerequisites are confirming the host project, migration execution strategy, and university identity contract.

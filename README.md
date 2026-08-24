# zeus.academia.3b

The third iteration of the Zeus Academia experiment

## SQL Server Setup

- The application and Shared Kernel verification flow use SQL Server for persistence and schema checks.
- Set `ZEUS_SQLSERVER_CONNECTION` when running on non-Windows hosts or CI environments that do not expose SQL Server LocalDB.
- On Windows, the verification script can fall back to SQL Server LocalDB `(localdb)\\MSSQLLocalDB` when no connection string is provided.
- Run the Shared Kernel verification script with:
  - `powershell -NoProfile -ExecutionPolicy Bypass -File eng/verify-shared-kernel-sqlserver.ps1`

## AI-Assisted Artifacts

- [Academia Slice Implementation Prompts](.github/prompts/academia/README.md) ([Log](ai-logs/2026/04/18/2026-04-18-academia-slice-implementation-prompts/conversation.md))
- [Academia Slice Execution Plan](.github/prompts/academia/execution-plan.md) ([Log](ai-logs/2026/04/18/2026-04-18-academia-slice-agents-and-execution-plan/conversation.md))
- [Backend Slice Implementer Agent Profile](.github/agents/backend-slice-implementer.agent.md) ([Log](ai-logs/2026/04/18/2026-04-18-academia-slice-agents-and-execution-plan/conversation.md))
- [ManageDegrees reference-data slice](src/features/ReferenceData/ManageDegrees/Zeus.Academia.Features.ReferenceData.ManageDegrees.csproj) ([Tests](tests/Features/ReferenceData/ManageDegrees/Zeus.Academia.Tests.Features.ReferenceData.ManageDegrees.csproj)) - canonical degree codes for registration and qualification workflows.
- [Phase 1 reference-data subagent workflow](CODE/2026-08-22-phase-1-reference-data-subagents.blog.md) ([ManageUniversities prompt](.github/prompts/academia-implementation/ep-1-3-manage-universities-implementation.prompt.md), [ProvisionExtension prompt](.github/prompts/academia-implementation/ep-1-4-provision-extension-implementation.prompt.md)) - explains both implementation prompts and how specialized subagents can execute them. ([Log](ai-logs/2026/08/23/2026-08-23-convert-reference-data-post-to-past-tense/conversation.md))
- [Blog Author Agent Profile](.github/agents/blog-author.agent.md) ([Log](ai-logs/2026/04/20/2026-04-20-blog-author-agent-conversion/conversation.md))
- [Academia Execution Plan](.github/models/workflows/academia-execution-plan.md) ([Log](ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md))
- [Academia Slice Implementation Prompts](.github/prompts/academia-implementation/README.md) ([Log](ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md))
- [Implementation Prompt Standards](.github/instructions/implementation-prompt.instructions.md) ([Log](ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md))
- [Implementation Prompt Instruction Prompt](.github/prompts/create-implementation-prompt-instructions.prompt.md) ([Log](ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md))
- [Create Academia Execution Plan Prompt](.github/prompts/create-academia-execution-plan.prompt.md) ([Log](ai-logs/2026/04/20/2026-04-20-create-academia-execution-plan-prompt/conversation.md))
- [Custom Agents Instruction Prompt](.github/prompts/create-custom-agents-instructions.prompt.md) ([Log](ai-logs/2026/02/25/2026-02-25-custom-agents-instructions-prompt/conversation.md))
- [Custom Agents Standards](.github/instructions/custom-agents.instructions.md) ([Log](ai-logs/2026/02/25/2026-02-25-custom-agents-instructions-generation/conversation.md))
- [Implementation Prompt Generation Prompt](.github/prompts/create-implementation-prompt-generation-instructions.prompt.md) ([Log](ai-logs/2026/04/18/2026-04-18-implementation-prompt-generation-prompt/conversation.md))
- [Implementation Prompt Generation Standards](.github/instructions/implementation-prompt-generation.instructions.md) ([Log](ai-logs/2026/04/18/2026-04-18-implementation-prompt-instructions/conversation.md))
- [Slice Coordinator Agent Profile](.github/agents/slice-coordinator.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Backend Domain Agent Profile](.github/agents/backend-domain.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Frontend Workflow Agent Profile](.github/agents/frontend-workflow.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Testing Verification Agent Profile](.github/agents/testing-verification.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Data Integration Doc Agent Profile](.github/agents/data-integration-doc.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Data Persistence Agent Profile](.github/agents/data-persistence.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Report Projection Agent Profile](.github/agents/report-projection.agent.md) ([Log](ai-logs/2026/04/20/6416bdb7-2948-42a3-9d26-dda894bf8ab7/conversation.md))
- [Product Manager Agent Prompt](.github/prompts/create-product-manager-agent.prompt.md) ([Log](ai-logs/2026/02/25/2026-02-25-product-manager-agent-prompt/conversation.md))
- [Product Manager Agent Profile](.github/agents/product-manager.agent.md) ([Log](ai-logs/2026/02/25/2026-02-25-product-manager-agent-prompt/conversation.md))
- [Prompt Engineer Agent Profile](.github/agents/prompt-engineer.agent.md) ([Log](ai-logs/2026/04/20/2026-04-20-prompt-engineer-agent-conversion/conversation.md))
- [Slice Verifier Agent Profile](.github/agents/slice-verifier.agent.md) ([Log](ai-logs/2026/04/18/2026-04-18-academia-slice-agents-and-execution-plan/conversation.md))

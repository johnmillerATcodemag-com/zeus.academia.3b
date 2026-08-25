# zeus.academia.3b

The third iteration of the Zeus Academia experiment

## SQL Server Setup

- The application and Shared Kernel verification flow use SQL Server for persistence and schema checks.
- Set `ZEUS_SQLSERVER_CONNECTION` when running on non-Windows hosts or CI environments that do not expose SQL Server LocalDB.
- On Windows, the verification script can fall back to SQL Server LocalDB `(localdb)\\MSSQLLocalDB` when no connection string is provided.
- Run the Shared Kernel verification script with:
  - `powershell -NoProfile -ExecutionPolicy Bypass -File eng/verify-shared-kernel-sqlserver.ps1`

## AI-Assisted Artifacts

- [Academia architecture issues and changes](docs/academia-architecture-issues-and-changes.md) - human-readable summary of the issues discovered and the changes made to address them. ([Log](ai-logs/2026/08/24/78bccef1-7df6-4b32-99b6-2cc4a743aecc/conversation.md))
- [Academia Slice Execution Plan](.github/prompts/academia/execution-plan.md) ([Log](ai-logs/2026/04/18/2026-04-18-academia-slice-agents-and-execution-plan/conversation.md))

# Copilot Instructions for `A2APOIntakeAgent` project

## Big picture
- This project hosts a minimal ASP.NET Core app (`Program.cs`) that boots a Semantic Kernel A2A TaskManager, exposes A2A endpoints, and wires in a custom `A2APOIntakeAgent`.
- `A2APOIntakeAgent` wraps a `ChatCompletionAgent` and translates A2A events into Semantic Kernel calls. Initialization depends on Azure OpenAI-like environment variables and logs key milestones.

## Essential workflows
- Build and run with the standard .NET CLI inside `src/Agents/A2APOIntakeAgent`:
  - `dotnet restore` (only needed after package changes)
  - `dotnet build` for CI-style validation
  - `dotnet run` to launch the local web host on the default Kestrel ports.
- Once running, hit `GET /health` to confirm the agent host is alive before invoking A2A routes.
- No automated tests exist today; if you add any, keep them under this project and wire them into `dotnet test`.

## Configuration & dependencies
- The agent requires these environment variables at startup:
  - `DEPLOYMENT_NAME`
  - `ENDPOINT`
  - `API_KEY`
If missing, run the command: source .env
This will set the required environment variables from the `.env` file.

- Packages come primarily from preview Semantic Kernel A2A builds (`Microsoft.SemanticKernel.Agents.*`) and the `A2A` preview surface; avoid downgrading or mixing release versions without checking compatibility.

## Coding patterns to follow
- Always create agents through `Kernel.CreateBuilder()` and attach them to the shared `TaskManager`; follow the pattern in `POProcessingAgent.InitializeAgent()` when adding new agents.
- When you introduce new request handlers, wire them via the TaskManager delegates (`OnMessageReceived`, `OnAgentCardQuery`) rather than directly in minimal APIs.
- Log with the injected `ILogger<T>`; avoid constructing new `LoggerFactory` instances unless absolutely necessary—`Program.cs` currently mixes patterns, so prefer DI when expanding.
- Keep agent prompt instructions within `"""` raw string literals to preserve formatting.
- For new HTTP endpoints, prefer minimal API style (`app.MapGet/Post`) and keep responses small JSON payloads or Semantic Kernel messages.

## Integration tips
- A2A endpoints are surfaced through helper extension methods (`MapA2A`, `MapHttpA2A`, `MapWellKnownAgentCard`) from `A2A.AspNetCore`; call them after attaching the agent.
- `ProcessMessageAsync` currently echoes back file metadata. Extend it by synthesizing Semantic Kernel completions through `_agent` to maintain the A2A contract (`A2AResponse`).
- Reuse `PurchaseOrder` helpers when constructing prompt context; they already compute totals and tax.

## Troubleshooting
- Despite some documentation, the Agent Card is located at this URL http://localhost:5000/.well-known/agent-card.json

## Housekeeping
- Update this doc when you add new agents, change environment requirements, or introduce custom build/test steps.
- If you add secrets or API keys, document required configuration here but keep actual values out of source control.

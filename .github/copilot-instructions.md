# Copilot instructions - ORUK Alternative Representations

## Build, test, and lint commands

Requires **.NET 10 SDK**.

```powershell
dotnet restore .\OrukAlternativeRepresentations.slnx
dotnet build .\OrukAlternativeRepresentations.slnx
dotnet test .\OrukAlternativeRepresentations.slnx
```

`OrukAlternativeRepresentations.slnx` does **not** include `src\OrukTransformer.Cli.Tests` in Debug build/test, so run it explicitly:

```powershell
dotnet test .\src\OrukTransformer.Cli.Tests\OrukTransformer.Cli.Tests.csproj
```

Run a single test with `--filter` (example):

```powershell
dotnet test .\src\OrukTransformer.Core.Tests\OrukTransformer.Core.Tests.csproj --filter "FullyQualifiedName~OrukToSchemaOrgTransformerTests.Transform_MinimalService_ProducesSchemaOrgDocument"
```

There is currently no dedicated lint command/workflow; use `dotnet build` as the analyzer/compile quality gate.

## High-level architecture

1. **Shared model contracts (`code\OrukModels`)**
   - ORUK entity models (`OrukModels.Models`) plus Schema.org JSON-LD output records (`OrukModels.SchemaOrg`).
2. **Transformation engine (`src\OrukTransformer.Core`)**
   - `OrukToSchemaOrgTransformer` maps one `OrukService` graph to one Schema.org `@graph` document and emits a field-level `TransformationReport` (VODIM).
3. **CLI pipeline (`src\OrukTransformer.Cli`)**
   - `RunCommand` orchestrates: fetch paged ORUK services -> transform each service -> merge JSON-LD nodes by `@id` -> write JSON-LD -> emit VODIM console/HTML report.
4. **Reusable ORUK HTTP access (`src\OrukApiClient`)**
   - Typed clients for `/services`, `/taxonomy_terms`, `/organizations`.
   - Handles both page-number pagination and RPDE cursor pagination (`next_url` / `next`).
5. **MCP server (`src\OrukTransformer.Mcp`)**
   - Loads feeds from `feeds.json`, resolves feed aliases via `IFeedRegistry`, fans out tool queries across feeds in parallel, and uses `TaxonomyCache` for per-feed term caching.

## Key conventions for this repository

- **`System.Text.Json` only** across all projects. Do not introduce Newtonsoft unless a hard dependency forces it.
- Follow **receive liberally / supply conservatively**:
  - tolerate incomplete/messy ORUK input (log and continue where possible),
  - omit uncertain Schema.org output fields instead of guessing values.
- **VODIM is part of core behavior**, not optional diagnostics: mapping code should record field classifications when adding/changing mappings.
- `feeds.json` supports:
  - string entries (`"https://.../v3"`), and
  - object entries (`{ "url": "...", "name": "...", "aliases": [...] }`).
- Always normalize feed URLs with repository helpers (`OrukUrlBuilder.EnsureBase`, `FeedRegistry.Resolve`) because callers may provide either feed base URLs or `/services` URLs.
- Pagination assumptions:
  - do not trust `total_pages` blindly,
  - support RPDE cursor links (`next_url`/`next`),
  - keep safety caps (`MaxPages`) and partial-failure tolerance.
- Proximity search expects ORUK `proximity` as `lat,long`; postcode strings are geocoded first (`PostcodesIoGeocoder`), then client-side distance fallback is applied.
- **Output channel discipline**:
  - CLI reserves stdout for JSON-LD when `--json-ld` is omitted (logs/VODIM suppressed there),
  - MCP reserves stdout for JSON-RPC and sends logs to stderr.
- `OrukService` captures unknown vendor fields via `[JsonExtensionData]`; preserve this extension mechanism when evolving models.
- Keep tests fixture-based and deterministic (`code\OrukModels.Tests\fixtures`, `src\OrukTransformer.Core.Tests\fixtures`); avoid live network calls in unit tests.

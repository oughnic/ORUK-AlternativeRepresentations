# OrukApiClient

Focused ORUK v3 API client library used by both `OrukTransformer.Mcp` and `OrukTransformer.Cli`.

## Purpose

This library provides typed interfaces and implementations for querying ORUK v3 REST endpoints. It has no knowledge of Schema.org, FHIR, MCP, or any transformation concern — its only job is to **fetch and filter ORUK data over HTTP**.

## Contents

| File | Purpose |
|------|---------|
| `OrukServiceQuery.cs` | Typed record for service search parameters (keyword, taxonomy, proximity, age, cost) |
| `IOrukServiceClient.cs` | Interface: `SearchAsync` (paginated, filtered) and `GetByIdAsync` |
| `IOrukTaxonomyClient.cs` | Interface: `GetAllTermsAsync` and `ResolveByLabel` |
| `OrukServiceClient.cs` | Concrete implementation with pagination, client-side filtering, tolerant deserialisation |
| `OrukTaxonomyClient.cs` | Fetches `/taxonomy_terms` and resolves user labels to term IDs |
| `Internal/OrukUrlBuilder.cs` | Constructs ORUK API URLs from base URL and query parameters |

## Key Design Decisions

- **Receive liberally:** Missing or unexpected fields are tolerated; a fallback case-insensitive deserialiser is tried before failing.
- **Client-side filtering fallback:** Taxonomy, age range, and cost filters are applied client-side if the endpoint doesn't support them as query parameters.
- **Base URL normalisation:** Both `https://example.org/v3` and `https://example.org/v3/services` are accepted as feed base URLs; the `/services` suffix is stripped automatically.
- **No transformation dependencies:** This library references only `OrukModels`, `Microsoft.Extensions.Http`, and `Microsoft.Extensions.Logging.Abstractions`.

## Registration (DI)

```csharp
services.AddHttpClient<OrukServiceClient>();
services.AddHttpClient<OrukTaxonomyClient>();
services.AddSingleton<IOrukServiceClient, OrukServiceClient>();
services.AddSingleton<IOrukTaxonomyClient, OrukTaxonomyClient>();
```

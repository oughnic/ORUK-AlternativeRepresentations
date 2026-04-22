# Technical Approach – ORUK to Schema.org Transformation Service

## Goal

Build a lightweight service that reads a JSON configuration file listing one or more **Open Referral UK (ORUK)** feed endpoint URLs, fetches the live data from each endpoint over HTTP, and produces a single, consolidated **Schema.org JSON-LD** response.

The service requires no UI.  It must be deployable as:

- A **Heroku** web dyno (alongside the iStandUK ORUK Validator), or
- An **Azure Function** (HTTP trigger), or
- An equivalent **GCP Cloud Function** or **AWS Lambda**.

The implementation language is **C#** targeting **.NET 8 (LTS)**, running on Linux in Docker or on Windows.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────┐
│                HTTP Request                       │
│  GET /jsonld  (optionally ?feed=<name>)           │
└─────────────────────┬────────────────────────────┘
                      │
             ┌────────▼────────┐
             │  Entry Point     │
             │  (HTTP handler)  │
             └────────┬────────┘
                      │
             ┌────────▼────────────────┐
             │  Feed Config Loader      │
             │  Reads feeds.json        │
             │  (list of endpoint URLs) │
             └────────┬────────────────┘
                      │
             ┌────────▼────────────────┐
             │  HTTP Feed Fetcher       │
             │  GETs each ORUK endpoint │
             │  via HttpClient          │
             └────────┬────────────────┘
                      │
             ┌────────▼────────┐
             │  ORUK Deserialise│
             │  to C# model     │
             └────────┬────────┘
                      │
             ┌────────▼──────────────────┐
             │  Transformer               │
             │  Maps ORUK entities        │
             │  to Schema.org types       │
             └────────┬──────────────────┘
                      │
             ┌────────▼────────┐
             │  JSON-LD Builder │
             │  Produces @graph  │
             └────────┬────────┘
                      │
             ┌────────▼────────┐
             │  HTTP Response   │
             │  Content-Type:   │
             │  application/    │
             │  ld+json         │
             └─────────────────┘
```

---

## Project Structure (proposed)

```
/
├── src/
│   └── OrukTransformer/
│       ├── OrukTransformer.csproj
│       ├── Program.cs                  # Minimal API host
│       ├── Models/
│       │   ├── Oruk/                   # C# POCOs matching ORUK JSON schema
│       │   └── SchemaOrg/              # C# POCOs for Schema.org JSON-LD output
│       ├── Services/
│       │   ├── IFeedConfigLoader.cs
│       │   ├── FileFeedConfigLoader.cs # Reads feeds.json (list of URLs)
│       │   ├── IFeedFetcher.cs
│       │   ├── HttpFeedFetcher.cs      # Fetches ORUK data from each URL via HttpClient
│       │   ├── ITransformer.cs
│       │   └── OrukToSchemaOrgTransformer.cs
│       └── Endpoints/
│           └── JsonLdEndpoint.cs
├── feeds.json                          # List of ORUK endpoint URLs (config)
├── tests/
│   └── OrukTransformer.Tests/
│       └── OrukTransformer.Tests.csproj
├── plan/                               # This directory
├── Dockerfile
├── docker-compose.yml
├── .gitignore
└── README.md
```

---

## Hosting Approaches

### Heroku

- Package as a minimal ASP.NET Core Minimal API application.
- `Program.cs` binds to `$PORT` environment variable (Heroku requirement).
- Deploy via Git push to Heroku remote or a Heroku container registry (Docker).
- `Procfile`: `web: dotnet OrukTransformer.dll`

### Azure Function

- Wrap the transformer in an `HttpTrigger` Azure Function.
- Use the .NET isolated worker model (`net8.0`).
- Deploy via `func azure functionapp publish` or GitHub Actions to Azure.

### GCP Cloud Function / AWS Lambda

- Use the same core transformer library.
- Add a thin adapter entry point per platform (GCP Functions Framework for .NET; AWS Lambda .NET runtime).
- Core business logic stays in a shared class library, platform adapters reference it.

---

## Feed Configuration

- A single `feeds.json` file at the repository root lists the ORUK endpoint URLs to aggregate:

```json
[
  "https://example-council.gov.uk/api/services",
  "https://another-provider.org.uk/oruk/services"
]
```

- At startup (or per-request) `FileFeedConfigLoader` reads this file and returns the list of URLs.
- `HttpFeedFetcher` issues an HTTP `GET` to each URL in turn using a shared `HttpClient` instance (respecting `IHttpClientFactory` best practices).
- A query parameter `?feed=<url-encoded-url>` allows callers to request a single named feed rather than the full consolidated set.
- If a feed URL is unreachable or returns a non-2xx status, the error is logged and that feed is skipped; remaining feeds continue to be processed.
- Duplicate service IDs across feeds are deduplicated — the first occurrence wins (sources are processed in the order listed in `feeds.json`).

---

## Transformation Pipeline

1. **Fetch** the ORUK data by issuing HTTP `GET` requests to each URL listed in `feeds.json`.
2. **Deserialise** each ORUK JSON response into typed C# models (generated from the iStandUK JSON Schema, or hand-authored POCOs).
3. **Validate** liberally: missing optional fields do not abort processing; they are omitted from output.  Unreachable or invalid feeds are logged and skipped.
4. **Map** each ORUK `Service` entity to a `GovernmentService` Schema.org node.  See [mapping.md](mapping.md) for field-level rules.
5. **Map** each ORUK `Organization` to a `schema:Organization` node.
6. **Map** each ORUK `Location` to a `schema:Place` node with a nested `PostalAddress` and optionally `GeoCoordinates`.
7. **Link** nodes: `GovernmentService.provider` references the `Organization` node by `@id`.
8. **Build** the `@graph` array and serialise to JSON-LD.

---

## Design Principles

| Principle | Application |
|-----------|-------------|
| **Receive liberally** | Accept ORUK feeds even when optional fields are absent or have unexpected types; log warnings rather than rejecting.  Tolerate individual feed URLs that are temporarily unavailable. |
| **Supply conservatively** | Only include fields in the JSON-LD output that have well-defined mappings and non-null values. |
| **Stateless** | The service holds no persistent state; `feeds.json` is the only configuration, and all data is fetched at request time. |
| **Testable** | Business logic (transformer) is injected as a dependency; entry point is thin. |
| **Observable** | Structured logging (Microsoft.Extensions.Logging) with configurable log levels. |

---

## Output Format

```json
{
  "@context": "https://schema.org",
  "@graph": [
    {
      "@type": "GovernmentService",
      "@id": "https://<base-url>/services/<oruk-id>",
      "name": "...",
      "description": "...",
      "provider": { "@id": "https://<base-url>/organisations/<oruk-org-id>" },
      ...
    },
    {
      "@type": "Organization",
      "@id": "https://<base-url>/organisations/<oruk-org-id>",
      "name": "...",
      ...
    }
  ]
}
```

`Content-Type: application/ld+json`

---

## Future Extensions

| Extension | Notes |
|-----------|-------|
| **FHIR output** | `GET /fhir/HealthcareService` returning a FHIR `Bundle`. |
| **MCP endpoint** | Model Context Protocol tool for AI agents to query services. |
| **Caching** | Cache transformed output with configurable TTL to avoid re-fetching on every request. |

---

## References

- ORUK JSON Schema: <https://github.com/iStandUK/prod-oruk-validator>
- Schema.org GovernmentService: <https://schema.org/GovernmentService>
- ASP.NET Core Minimal API: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis>
- Azure Functions .NET isolated: <https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide>
- Heroku .NET: <https://devcenter.heroku.com/articles/getting-started-with-dotnet>

# Copilot Instructions – ORUK Alternative Representations

## Role of This Repository

This repository explores how data published in the **Open Referral UK (ORUK)** format can be transformed and served in alternative standard representations:

- **Schema.org JSON-LD** – for search-engine and AI-agent discoverability.
- **HL7 FHIR HealthcareService** – for integration with NHS and clinical systems.
- **Model Context Protocol (MCP)** – for LLM / AI agent toolchain access.

The primary deliverable is a **transformation service** with no user interface.  It reads a `feeds.json` configuration file containing a list of ORUK API endpoint URLs, fetches the live data from each endpoint, and returns a consolidated JSON-LD (or FHIR, or MCP) response.

---

## Delivery Model

The service must be deployable in any of the following environments without code changes (only configuration differs):

| Platform | Mechanism |
|----------|-----------|
| **Heroku** | ASP.NET Core Minimal API; bind to `$PORT`; `Procfile` entry. |
| **Azure Function** | HTTP-triggered Azure Function using the .NET 8 isolated worker model. |
| **GCP Cloud Function** | GCP Functions Framework for .NET adapter. |
| **AWS Lambda** | AWS Lambda .NET runtime adapter. |
| **Docker / Linux** | Multi-stage `Dockerfile`; `linux/amd64` target; runs via `dotnet` CLI. |
| **Windows** | Standard .NET 8 self-contained executable. |

The core business logic lives in a shared class library (`OrukTransformer.Core`).  Each deployment target references this library via a thin adapter project.

---

## Technology Stack

- **Language:** C# 12
- **Runtime:** .NET 8 LTS
- **HTTP framework:** ASP.NET Core Minimal API (for Heroku / Docker deployments)
- **Serialisation:** `System.Text.Json` (not Newtonsoft.Json unless unavoidable)
- **HTTP client:** `IHttpClientFactory` / `HttpClient` (named clients registered in DI)
- **Logging:** `Microsoft.Extensions.Logging` with structured logging
- **Testing:** xUnit + NSubstitute (or Moq) for unit tests; no UI test frameworks needed
- **CI:** GitHub Actions

---

## C# Coding Practices

### General

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Use `record` types for immutable data-transfer objects (DTOs); use `class` only when mutability is required.
- Prefer `IReadOnlyList<T>` / `IReadOnlyDictionary<K,V>` over mutable collection interfaces in method signatures.
- Always use `nullable` reference types (`<Nullable>enable</Nullable>`).  Annotate nullability explicitly; do not suppress warnings with `!` unless genuinely impossible to be null.
- Use `async`/`await` throughout; never block on async code (`.Result`, `.Wait()`).
- Favour dependency injection over `static` methods or singletons.

### Naming

| Item | Convention | Example |
|------|------------|---------|
| Interface | `I` prefix + PascalCase | `IFeedFetcher` |
| Class | PascalCase | `HttpFeedFetcher` |
| Method | PascalCase | `FetchAsync` |
| Private field | `_camelCase` | `_httpClient` |
| Local variable | camelCase | `serviceNode` |
| Constant | PascalCase | `DefaultCurrency` |

### Architecture

- **Thin entry points:** `Program.cs` and adapter projects contain only wiring (DI registration, routing).  No business logic.
- **Single responsibility:** Each class has one clear purpose; split at natural seams (config loading, HTTP fetching, deserialisation, transformation, serialisation).
- **Interfaces for every service:** Always program to an interface so that implementations can be swapped in tests and alternative deployment targets.
- **No `static` state:** Global or static mutable state is prohibited.

### Error Handling

- Catch exceptions at the boundary (HTTP handler, feed fetcher); re-throw only if the error is unrecoverable.
- Use `ILogger` to record warnings for individual feed failures; do not let a single bad feed abort the entire request.
- Return appropriate HTTP status codes: `200 OK` with partial results is preferable to `500 Internal Server Error` when at least one feed succeeds.

### Receive Liberally / Supply Conservatively

This is the guiding principle for all data handling:

- **Receive liberally:** Tolerate missing, null, or unexpected values in incoming ORUK responses.  Use `JsonIgnoreCondition.WhenWritingNull` and `[JsonIgnore]` where appropriate.  Log anomalies as warnings, never throw.
- **Supply conservatively:** Only include a property in the output JSON-LD if it has a well-defined, non-null, non-empty value.  Omit rather than guess.

### HttpClient Usage

- Register all `HttpClient` instances via `IHttpClientFactory` in `Program.cs`.
- Set sensible timeouts (e.g. 30 s per feed request).
- Add a `User-Agent` header identifying the service version.
- Do not disable SSL certificate validation.

### Configuration

- All configuration (feed URLs, base URL, timeouts) flows through `IConfiguration` / `IOptions<T>` — never hardcoded.
- `feeds.json` is the primary configuration file for endpoint URLs:

```json
[
  "https://example-council.gov.uk/api/services",
  "https://another-provider.org.uk/oruk/services"
]
```

### Testing

- Unit-test the transformer independently of HTTP: pass pre-loaded ORUK model objects directly.
- Use `HttpMessageHandler` mocks (e.g. `MockHttpMessageHandler` from `RichardSzalay.MockHttp`) to test the feed fetcher without real network calls.
- Keep test data as small, representative JSON fixtures under `tests/fixtures/`.
- Aim for high coverage of the mapping logic in `OrukToSchemaOrgTransformer`.

---

## Related Standards

See [`plan/standards/`](plan/standards/README.md) for full descriptions of:

- [HSDS](plan/standards/hsds.md)
- [Open Referral UK](plan/standards/open-referral-uk.md)
- [Schema.org](plan/standards/schema-org.md)
- [HL7 FHIR HealthcareService](plan/standards/hl7-fhir.md)

## Architecture and Mapping

- [Technical approach](plan/approach.md)
- [ORUK → Schema.org field mapping](plan/mapping.md)

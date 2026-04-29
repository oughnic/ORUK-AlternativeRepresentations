# OrukModels.Tests

xUnit test project for the `OrukModels` class library.

## Purpose

Verifies that `OrukModels` model types correctly deserialise real ORUK v3 feed data.
All tests use static JSON fixture files — no live HTTP calls are made.

## Structure

| Path | Description |
|------|-------------|
| `OrukServiceDeserializationTests.cs` | Deserialisation tests for `OrukService` and `OrukPage<OrukService>` |
| `fixtures/` | Static ORUK JSON fixture files sourced from live feeds |

## Fixtures

Fixture files are sourced from the Bristol Open Place Directory
(`https://bristol.openplace.directory/o/OpenReferralService/v3`) and checked in as
static JSON to ensure tests are deterministic and require no network access.

| File | Source | Description |
|------|--------|-------------|
| `fixtures/bristol-service-1625ip.json` | `GET /services/2a3483ea-8966-4f50-9dc3-7bafa1e5c623` | Single fully-nested service with contacts, service areas, and OPD extensions |
| `fixtures/bristol-services-page1.json` | `GET /services?per_page=3` | Paged response with 3 services |

## Running Tests

```bash
dotnet test
```

Or from the repository root:

```bash
dotnet test code/OrukModels.Tests/OrukModels.Tests.csproj
```

## Dependencies

| Package | Purpose |
|---------|---------|
| `xunit` | Test framework |
| `NSubstitute` | Mocking library (available for future tests) |
| `Microsoft.NET.Test.Sdk` | .NET test runner integration |

## Maintenance

> **README maintenance:** This `README.md` must be updated whenever new test classes or
> fixture files are added, or when existing tests change significantly.

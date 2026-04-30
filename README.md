# ORUK-AlternativeRepresentations

A repository exploring how Open Referral UK (ORUK) data can be transformed and served in alternative standard representations, including Schema.org JSON-LD, HL7 FHIR HealthcareService, and Model Context Protocol (MCP) APIs.

## Purpose

Local authorities, NHS trusts, and third-sector organisations publish service-directory data in the [Open Referral UK](https://openreferraluk.org/) format.  This project investigates how that same data can be exposed in other widely-used formats so that:

- Search engines and AI agents can discover services via **Schema.org** `LocalBusiness` / `GovernmentService` markup.
- Health systems can consume service information via **HL7 FHIR** `HealthcareService` resources.
- AI/LLM toolchains can query the data via a **Model Context Protocol (MCP)** endpoint.

The initial focus is on reading a JSON configuration file that lists one or more ORUK feed endpoint URLs, fetching the live data from each endpoint, and producing a consolidated **JSON-LD** response conformant with Schema.org.

## Structure

| Path | Description |
|------|-------------|
| `code/` | All source code (see [`code/README.md`](code/README.md)) |
| `code/OrukModels/` | Core ORUK v3 C# model class library |
| `code/OrukModels.Tests/` | xUnit tests; includes Bristol OPD fixture data |
| `plan/` | Planning documents: approach, mapping, and standards research |
| `plan/standards/` | Descriptions of each related standard |
| `plan/approach.md` | Technical approach to building the transformation service |
| `plan/mapping.md` | Field-level mapping from ORUK to Schema.org |

## Unmapped ORUK Fields

The Schema.org transformation is **lossy** for the following ORUK fields.  No suitable Schema.org property exists, so the value is intentionally omitted from the output.  The VODIM report records each occurrence as `Other` to flag the data loss.

| ORUK entity | Field | Reason |
|-------------|-------|--------|
| `Service` | `email` | `schema:GovernmentService` does not have a standard email property and its `contactPoint` collection is not a permitted property either; there is no schema.org mapping that faithfully represents a service-level contact email. |

If you need these fields in downstream systems, consume the raw ORUK feed directly or extend the transformer with a custom `additionalProperty` fallback.

## Related Projects

- [iStandUK ORUK Validator](https://github.com/iStandUK/prod-oruk-validator) – JSON Schema validator for ORUK APIs (the canonical schema source used by this project).
- [OpenReferral](https://openreferral.org/) – The international parent standard (HSDS).

## Getting Started

The code is in the [`code/`](code/README.md) directory.

```bash
# Build
dotnet build code/OrukAlternativeRepresentations.slnx

# Test
dotnet test code/OrukAlternativeRepresentations.slnx
```

Further documentation and planning materials are in the [`plan/`](plan/README.md) directory.

## Licence

See [LICENSE](LICENSE).

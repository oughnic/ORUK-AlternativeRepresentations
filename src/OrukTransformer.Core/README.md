# OrukTransformer.Core

Class library containing the ORUK → Schema.org transformation pipeline.

## Responsibilities

- Transform a fully-populated `OrukService` (and its navigation graph) into a Schema.org JSON-LD `@graph` document (`SchemaOrgDocument`).
- Produce a field-by-field **VODIM data-quality report** (`TransformationReport`) alongside every transformation.
- Provide shared plain-text normalization helpers for ORUK narrative fields (`OrukPlainText`) and model-level convenience extensions (for example, `service.DescriptionPlain()`).

## Key Namespaces

| Namespace | Purpose |
|-----------|---------|
| `OrukTransformer.Core.Mapping` | Transformer interface, implementation, options, and result types |
| `OrukTransformer.Core.Vodim` | VODIM classification enum, field record, and report accumulator |
| `OrukTransformer.Core` | Shared plain-text normalization (`OrukPlainText`) and ORUK model extension methods |

## Entry Point

```csharp
IOrukToSchemaOrgTransformer transformer = new OrukToSchemaOrgTransformer();

TransformationResult result = transformer.Transform(orukService, new TransformationOptions
{
    BaseUrl       = "https://services.example.org",
    DefaultCurrency = "GBP",
});

// JSON-LD document
string json = JsonSerializer.Serialize(result.Document, SchemaOrgSerializerOptions.Default);

// VODIM quality report
Console.WriteLine(result.Report.Summary());
```

## VODIM Data Quality

Each field that could potentially be mapped receives exactly one classification:

| Code | Meaning |
|------|---------|
| **V** Valid   | Present, valid in ORUK source, maps cleanly to valid Schema.org value |
| **O** Other   | Present but outside the expected controlled vocabulary (source or target) |
| **D** Default | Source absent; a default value was applied and emitted |
| **I** Invalid | Present but fails format/range validation; target property omitted |
| **M** Missing | Absent in source; no default applied; target property omitted |

## Mapping Coverage

Every ORUK entity type that carries mappable content is handled:

| ORUK entity | Schema.org output |
|-------------|-------------------|
| `OrukService` | `GovernmentService` |
| `OrukOrganization` | `Organization` |
| `OrukLocation` (via `ServiceAtLocation`) | `Place` |
| `OrukSchedule` | `OpeningHoursSpecification` (one per RRULE day code) |
| `OrukContact` + `OrukPhone` | `ContactPoint` |
| `OrukCostOption` | `Offer` |
| `OrukEligibility` / age fields | `Audience` / `PeopleAudience` |
| `OrukServiceArea` | `AdministrativeArea` |
| `OrukLanguage` | `Language` |
| `OrukAccessibility` | `LocationFeatureSpecification` |
| `OrukExternalIdentifier` (UPRN) | `identifier` (`PropertyValue`) |
| `OrukAttribute` / `OrukTaxonomyTerm` | `additionalType` / `keywords` |

Fields with no Schema.org equivalent (e.g. `status`, `assured_date`, `assurer_email`) are preserved as `additionalProperty` (`PropertyValue`) nodes.

## Dependencies

- `OrukModels` — ORUK v3 entity models and Schema.org output records

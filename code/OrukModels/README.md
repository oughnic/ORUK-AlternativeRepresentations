# OrukModels

Core class library providing C# model types for Open Referral UK (ORUK) v3 / HSDS v3.0 data.

## Purpose

This library defines the entity model used to deserialise ORUK v3 API responses and serve as
the input to all transformation pipelines (Schema.org, FHIR, MCP).  It has no dependencies on
ASP.NET Core or any HTTP framework, and is intentionally thin — pure model classes only.

## Design Principles

- **Receive liberally:** All optional ORUK fields are nullable (`string?`, `int?`, etc.).
  Missing fields are tolerated during deserialisation; no exceptions are thrown for absent values.
- **EF-ready:** Entity model types use `class` with mutable `{ get; set; }` properties and `virtual`
  navigation properties to support future Entity Framework Core integration without a schema migration.
  Primary keys use `string` rather than `Guid`.
- **Pure value objects use `record`:** Immutable response wrappers and extension types
  (e.g. `OrukPage<T>`, `OrukExtensionProperty`) use `record` with `{ get; init; }` properties.
- **System.Text.Json:** All JSON mapping uses `[JsonPropertyName]` attributes.
  `Newtonsoft.Json` is not used.

## Entity Model

```mermaid
classDiagram
    OrukService "1" --> "0..1" OrukOrganization : organization
    OrukService "1" --> "0..1" OrukProgram : program
    OrukService "1" --> "*" OrukContact : contacts
    OrukService "1" --> "*" OrukPhone : phones
    OrukService "1" --> "*" OrukSchedule : schedules
    OrukService "1" --> "*" OrukServiceArea : service_areas
    OrukService "1" --> "*" OrukServiceAtLocation : service_at_locations
    OrukService "1" --> "*" OrukLanguage : languages
    OrukService "1" --> "*" OrukCostOption : cost_options
    OrukService "1" --> "*" OrukEligibility : eligibility
    OrukService "1" --> "*" OrukRequiredDocument : required_documents
    OrukService "1" --> "*" OrukFunding : funding
    OrukService "1" --> "*" OrukAttribute : attributes
    OrukServiceAtLocation "1" --> "0..1" OrukLocation : location
    OrukLocation "1" --> "*" OrukAddress : physical_addresses
    OrukLocation "1" --> "*" OrukAccessibility : accessibility
    OrukLocation "1" --> "*" OrukExternalIdentifier : external_identifiers
    OrukContact "1" --> "*" OrukPhone : phones
    OrukAttribute "1" --> "0..1" OrukTaxonomyTerm : taxonomy_term
    OrukTaxonomyTerm "1" --> "0..1" OrukTaxonomy : taxonomy_detail
```

## Models

| Class | ORUK entity | Notes |
|-------|-------------|-------|
| `OrukPage<T>` | Paged response wrapper | Record type; wraps `contents[]` |
| `OrukService` | `service` | Core service entity |
| `OrukOrganization` | `organization` | Legal entity delivering services |
| `OrukLocation` | `location` | Physical address / geographic point |
| `OrukAddress` | `address` | Postal or physical address |
| `OrukContact` | `contact` | Contact details for a service/org/location |
| `OrukPhone` | `phone` | Telephone number |
| `OrukSchedule` | `schedule` | Opening hours; supports RFC 5545 recurrence |
| `OrukServiceAtLocation` | `service_at_location` | Service–location join entity |
| `OrukServiceArea` | `service_area` | Geographic coverage (ONS codes) |
| `OrukCostOption` | `cost_option` | Pricing / fee information |
| `OrukEligibility` | `eligibility` | Access criteria |
| `OrukLanguage` | `language` | Language spoken at service/location |
| `OrukTaxonomyTerm` | `taxonomy_term` | Classification term |
| `OrukTaxonomy` | `taxonomy` | Classification scheme |
| `OrukAttribute` | `attribute` | Link between entity and taxonomy term |
| `OrukRequiredDocument` | `required_document` | Document required to access a service |
| `OrukFunding` | `funding` | Funding source |
| `OrukProgram` | `program` | Programme grouping services |
| `OrukAccessibility` | `accessibility` | Accessibility feature at a location |
| `OrukExternalIdentifier` | `external_identifier` | UPRN, USRN, etc. |
| `OrukMetadata` | `metadata` | Change audit trail |
| `OrukExtensionProperty` | _(any unrecognised field)_ | Generic vendor extension: namespace + key + raw JSON value |

## Extension Mechanism

ORUK feeds from some publishing platforms include proprietary fields not defined in the HSDS v3
specification (for example, PlaceCube's `pc_metadata` and `pc_targetAudience` fields in the
Bristol Open Place Directory).

Rather than introducing bespoke C# types for each vendor's extensions, `OrukService` uses the
standard `System.Text.Json` `[JsonExtensionData]` attribute to capture all unrecognised JSON
properties automatically.  These are then surfaced through the typed `Extensions` property as a
list of `OrukExtensionProperty` records.

### `OrukExtensionProperty`

| Property | Type | Description |
|----------|------|-------------|
| `Namespace` | `string` | Vendor prefix derived by splitting the field name on the first `_` (e.g. `"pc"` from `pc_metadata`). Empty string if no prefix. |
| `Key` | `string` | The remainder after the prefix (e.g. `"metadata"` from `pc_metadata`). |
| `RawKey` | `string` | The original JSON field name exactly as received (e.g. `"pc_metadata"`). |
| `Value` | `JsonElement` | Raw JSON value — scalar, object, or array — without loss of fidelity. |

### Example (Bristol OPD / PlaceCube feed)

```csharp
// Retrieve the PlaceCube metadata extension
var pcMeta = service.Extensions
    .SingleOrDefault(e => e.Namespace == "pc" && e.Key == "metadata");

// Read a scalar field from the nested object
var assuredBy = pcMeta?.Value.GetProperty("assured_by").GetString();

// Retrieve the PlaceCube target-audience array extension
var audiences = service.Extensions
    .SingleOrDefault(e => e.Namespace == "pc" && e.Key == "targetAudience");

var audienceTypes = audiences?.Value.EnumerateArray()
    .Select(el => el.GetProperty("audienceType").GetString())
    .ToList();
```

This approach works for any vendor prefix and any JSON value shape without requiring model changes.

```csharp
using System.Text.Json;
using OrukModels.Models;

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// Deserialise a paged ORUK response
var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, options);

// Deserialise a single service
var service = JsonSerializer.Deserialize<OrukService>(json, options);
```

## Maintenance

> **README maintenance:** This `README.md` must be updated whenever a new model class is added,
> an existing class changes significantly, or the design principles are revised.

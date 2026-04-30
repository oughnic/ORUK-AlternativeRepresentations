# OrukTransformer.Cli

A .NET 10 command-line application that fetches a live **Open Referral UK (ORUK) v3** service-directory endpoint, transforms each service to **Schema.org JSON-LD**, and reports **VODIM** data-quality metrics.

## Usage

```
oruk-transformer --oruk-url <url> [--json-ld <file>] [--max-records <n>] [--verbose]
```

### Options

| Option | Type | Required | Default | Description |
|---|---|---|---|---|
| `--oruk-url` | URI | Yes | — | URL of the ORUK v3 `GET /services` endpoint |
| `--json-ld` | file path | No | stdout | Output file for the JSON-LD; omit to write to stdout |
| `--max-records` | int | No | `50` | Max services to retrieve; values < 1 = no limit |
| `--verbose` | flag | No | `false` | Emit per-service VODIM field-level detail |
| `--data-quality-report` | file path | No | — | Write an xHTML5 data-quality HTML report to this file |

### Examples

```bash
# Write JSON-LD to stdout, VODIM summary to stderr
oruk-transformer --oruk-url https://bristol.openplace.directory/o/OpenReferralService/v3/services

# Write JSON-LD to a file, VODIM summary to stdout
oruk-transformer \
  --oruk-url https://bristol.openplace.directory/o/OpenReferralService/v3/services \
  --json-ld output.jsonld

# Retrieve up to 200 services with full VODIM field detail
oruk-transformer \
  --oruk-url https://bristol.openplace.directory/o/OpenReferralService/v3/services \
  --json-ld output.jsonld \
  --max-records 200 \
  --verbose

# No record limit (fetch all)
oruk-transformer \
  --oruk-url https://bristol.openplace.directory/o/OpenReferralService/v3/services \
  --json-ld all.jsonld \
  --max-records 0

# Generate an HTML data-quality report alongside the JSON-LD
oruk-transformer \
  --oruk-url https://bristol.openplace.directory/o/OpenReferralService/v3/services \
  --json-ld output.jsonld \
  --data-quality-report oruk-schema_org.html
```

## Output

### JSON-LD

A consolidated Schema.org `@graph` document in `application/ld+json` format, containing `GovernmentService`, `Organization`, and `Place` nodes.

### VODIM Report

Always printed after the transformation.  When `--json-ld` is supplied the report goes to **stdout**; otherwise it goes to **stderr** (to keep the JSON-LD output clean when piped).

**Summary (always shown):**

```
VODIM Summary — 42 service(s) transformed from https://example.org/services
  V Valid     :   1260  (71%)
  O Other     :     84  ( 5%)
  D Default   :    126  ( 7%)
  I Invalid   :     42  ( 2%)
  M Missing   :    252  (14%)
  Total fields:   1764
```

**Verbose additions (`--verbose`), per service with issues:**

```
--- Service abc-123 ---
[O] service.status → GovernmentService.additionalProperty (source: "pending") — unrecognised ORUK status
[I] service.schedules[0].opens_at → OpeningHoursSpecification.opens (source: "9am") — not ISO HH:mm
```

### HTML Data-Quality Report (`--data-quality-report`)

When `--data-quality-report <file>` is supplied, an xHTML5 data-quality report is written to the specified file in addition to the standard console VODIM output.

The report lists every distinct ORUK field path found across all services as an `<h2>` heading.  Under each heading, a VODIM metric breakdown table and a de-duplicated list of issue messages (with occurrence counts) are shown.  Instance-specific details such as individual field values are deliberately omitted to avoid data leakage.

The report uses embedded iStandUK-branded CSS and requires no external resources.

## Architecture

| Component | Responsibility |
|---|---|
| `Program.cs` | `System.CommandLine` wiring, DI wiring |
| `RunCommand` | Orchestrates fetch → transform → merge → write → report |
| `OrukFeedPageFetcher` | Pages through `GET /services?page=N&per_page=100` |
| `OrukToSchemaOrgTransformer` | Maps ORUK entities to Schema.org nodes (from `OrukTransformer.Core`) |
| `JsonLdMerger` | Merges per-service documents; deduplicates nodes by `@id` |
| `JsonLdWriter` | Serialises `SchemaOrgDocument` to file or stdout |
| `VodimReporter` | Formats VODIM summary and optional per-service detail |
| `HtmlDataQualityReportWriter` | Writes the xHTML5 data-quality HTML report |

## Paging

Requests use `per_page=100` (capped at the ORUK endpoint's own maximum) to minimise round-trips.  If `--max-records` is between 1 and 100, the first request uses `per_page=<max-records>` to avoid over-fetching.  A failed page is logged as a warning and skipped; the fetch continues with the next page.

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | Success — at least one service transformed |
| `1` | No services could be retrieved from the endpoint |

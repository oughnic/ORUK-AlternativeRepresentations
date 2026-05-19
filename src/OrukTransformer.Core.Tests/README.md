# OrukTransformer.Core.Tests

xUnit test project for `OrukTransformer.Core`.

## Test Coverage

| Test class | What is tested |
|------------|---------------|
| `Mapping/OrukToSchemaOrgTransformerTests` | Full transformer; 54 tests |
| `PlainTextNormalizationTests` | Shared plain-text normalizer and model extension methods |

### Test categories

- **Output document structure** — graph contains the expected node types
- **`@id` URI construction** — service, organisation, and location URIs
- **VODIM Valid** — name, URL, email, status, date, age, language, geo-coordinates, currency
- **VODIM Missing** — null/empty fields correctly omitted and recorded
- **VODIM Invalid** — malformed URL, email, date, out-of-range latitude, negative price
- **VODIM Other** — unrecognised ORUK status, sentinel ages (−1), long-form day names, non-standard fields
- **VODIM Default** — missing currency defaults to GBP
- **Schedule expansion** — RRULE `byday` values (short-form, long-form, ordinal, invalid) expanded to one `OpeningHoursSpecification` per day
- **CostOption → Offer** — free, numeric, negative (invalid), missing currency
- **Eligibility → Audience / PeopleAudience** — age constraints, description fallback, sentinel −1 handling
- **ServiceArea → AdministrativeArea** — multiple areas
- **Organization** — logo, sameAs, website fallback, legal_status
- **End-to-end Bristol OPD fixture** — real feed data; no exceptions, correct graph structure

## Fixtures

| File | Source |
|------|--------|
| `fixtures/bristol-service-1625ip.json` | Bristol Open Place Directory live feed (1625 Independent People) |

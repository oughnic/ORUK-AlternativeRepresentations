# OrukTransformer.Cli.Tests

xUnit unit-test project for `OrukTransformer.Cli`.

## Test Coverage

| Test class | What is tested |
|---|---|
| `OrukFeedPageFetcherTests` | Pagination logic — correct page/per_page parameters, stopping at `maxRecords`, handling HTTP errors on mid-run pages, aborting on first-page failure |
| `JsonLdMergerTests` | Deduplication of `@graph` nodes by `@id`, nodes without `@id` always included |
| `VodimReporterTests` | Summary text formatting for zero, one, and many reports |
| `RunCommandTests` | Pipeline orchestration behaviour, including VODIM report suppression when writing JSON-LD to stdout |
| `CliOutputModePolicyTests` | Stdout-only mode option-policy validation and effective log-level resolution |
| `OrukServiceClientTests` | Feed endpoint discovery fallback: try configured base URL first, then retry with `/services` when first-page results are empty |

## Running Tests

```bash
dotnet test src/OrukTransformer.Cli.Tests/
```

# OrukTransformer.Cli.Tests

xUnit unit-test project for `OrukTransformer.Cli`.

## Test Coverage

| Test class | What is tested |
|---|---|
| `OrukFeedPageFetcherTests` | Pagination logic — correct page/per_page parameters, stopping at `maxRecords`, handling HTTP errors on mid-run pages, aborting on first-page failure |
| `JsonLdMergerTests` | Deduplication of `@graph` nodes by `@id`, nodes without `@id` always included |
| `VodimReporterTests` | Summary text formatting for zero, one, and many reports |

## Running Tests

```bash
dotnet test src/OrukTransformer.Cli.Tests/
```

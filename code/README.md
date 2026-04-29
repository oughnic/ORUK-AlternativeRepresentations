# Code

This directory contains all source code for the ORUK Alternative Representations project.

## Structure

| Path | Description |
|------|-------------|
| [`OrukModels/`](OrukModels/README.md) | Core C# class library — ORUK v3 entity models |
| [`OrukModels.Tests/`](OrukModels.Tests/README.md) | xUnit test project for `OrukModels` |
| `OrukAlternativeRepresentations.slnx` | .NET solution file |

## Planned additions

| Path | Description |
|------|-------------|
| `OrukTransformer.Core/` | Business logic: ORUK → Schema.org transformation |
| `OrukTransformer.Api/` | ASP.NET Core Minimal API host (Heroku / Docker) |
| `OrukTransformer.AzureFunction/` | Azure Function adapter |

## Getting Started

```bash
# Build the solution
dotnet build OrukAlternativeRepresentations.slnx

# Run tests
dotnet test OrukAlternativeRepresentations.slnx
```

## Requirements

- .NET 10 SDK — [download](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## Maintenance

> **README maintenance:** This `README.md` must be kept up to date whenever the directory
> structure or project setup changes.  Any new project added to the solution must be
> listed in the table above.

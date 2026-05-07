# OrukTransformer.Mcp

An [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) server that allows AI agents — such as GitHub Copilot, Claude, and ChatGPT — to query live [Open Referral UK](https://openreferraluk.org/) service directories using natural language.

## Overview

This project implements an MCP server using the [official C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk). It exposes four tools that an AI agent can call to help a user find community services, understand eligibility, and get location and contact details.

## MCP Tools

| Tool | Description |
|------|-------------|
| `search_services` | Search across all configured ORUK feeds with keyword, location, age, and cost filters. |
| `get_service_detail` | Retrieve full details (opening times, accessibility, eligibility, cost) for a specific service. |
| `list_taxonomy_terms` | Browse the taxonomy categories used by a feed. |
| `resolve_taxonomy_label` | Translate a plain-language phrase into taxonomy term IDs. |

## Running in Development (stdio)

### Prerequisites

- .NET 10 SDK
- An ORUK v3 API endpoint (e.g. [Bristol Open Place Directory](https://bristol.openplace.directory/))

### Configuration

1. Edit `feeds.json` at the repository root to list the ORUK endpoints to query:

```json
[
  "https://bristol.openplace.directory/o/OpenReferralService/v3"
]
```

2. Optionally adjust `appsettings.json`:

```json
{
  "Mcp": {
    "MaxResultsPerQuery": 20,
    "TaxonomyCacheTtlMinutes": 60
  }
}
```

### Start the server

```bash
dotnet run --project src/OrukTransformer.Mcp
```

The server communicates over **stdin/stdout** using the MCP JSON-RPC protocol. Logs are written to **stderr** so they do not interfere with the protocol.

### Configure Claude Desktop

Add to `claude_desktop_config.json` (`%APPDATA%\Claude\claude_desktop_config.json` on Windows):

```json
{
  "mcpServers": {
    "oruk": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\ORUK-AlternativeRepresentations\\src\\OrukTransformer.Mcp"
      ]
    }
  }
}
```

Or point at the compiled executable:

```json
{
  "mcpServers": {
    "oruk": {
      "command": "C:\\path\\to\\OrukTransformer.Mcp.exe"
    }
  }
}
```

### Test with MCP Inspector

```bash
npx @modelcontextprotocol/inspector dotnet run --project src/OrukTransformer.Mcp
```

## Project Structure

```
OrukTransformer.Mcp/
├── Program.cs                  # Host setup, DI wiring, MCP server startup
├── McpOptions.cs               # Configuration options (bound from appsettings.json)
├── appsettings.json            # Default configuration
├── Config/
│   └── FeedsLoader.cs          # Parses feeds.json into a list of feed URIs
├── Models/
│   └── ServiceWithOrigin.cs    # Internal record wrapping a service with its feed URL
├── Taxonomy/
│   ├── ITaxonomyCache.cs       # Caching interface for taxonomy terms
│   └── TaxonomyCache.cs        # TTL-based in-memory taxonomy cache
└── Tools/
    ├── OrukServiceSearchTool.cs  # search_services MCP tool
    ├── OrukServiceDetailTool.cs  # get_service_detail MCP tool
    └── OrukTaxonomyTool.cs       # list_taxonomy_terms / resolve_taxonomy_label tools
```

## Dependencies

| Package | Purpose |
|---------|---------|
| `ModelContextProtocol` 1.2.0 | Official C# MCP SDK |
| `Microsoft.Extensions.Hosting` 10.x | Generic host, DI, configuration |
| `OrukApiClient` (project ref) | ORUK HTTP client with pagination and filtering |
| `OrukModels` (project ref) | ORUK v3 entity model classes |

## Architecture

```mermaid
flowchart TD
    Agent["AI Agent\n(Claude / Copilot / ChatGPT)"]
    MCP["OrukTransformer.Mcp\n(MCP Server — stdio)"]
    Cache["TaxonomyCache\n(in-memory, TTL-based)"]
    Client["OrukApiClient\n(IOrukServiceClient\nIOrukTaxonomyClient)"]
    Feed1["ORUK Feed 1\n(e.g. Bristol)"]
    Feed2["ORUK Feed 2\n(e.g. Council)"]

    Agent -- "MCP JSON-RPC\nover stdio" --> MCP
    MCP --> Cache
    MCP --> Client
    Cache --> Client
    Client --> Feed1
    Client --> Feed2
```

## Roadmap

- [ ] HTTP/SSE transport for hosted deployment
- [ ] Multi-feed result ranking and deduplication improvements
- [ ] Postcode-to-coordinates geocoding for proximity search
- [ ] Streaming results via MCP resource subscriptions

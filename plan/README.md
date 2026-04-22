# Plan

This directory contains planning and design documentation for the ORUK Alternative Representations project.

## Contents

| Document | Description |
|----------|-------------|
| [standards/README.md](standards/README.md) | Overview of related standards with relationship diagram |
| [standards/hsds.md](standards/hsds.md) | Human Services Data Specification (HSDS) |
| [standards/open-referral-uk.md](standards/open-referral-uk.md) | Open Referral UK (ORUK) |
| [standards/schema-org.md](standards/schema-org.md) | Schema.org as it relates to service directories |
| [standards/hl7-fhir.md](standards/hl7-fhir.md) | HL7 FHIR HealthcareService resource |
| [approach.md](approach.md) | Technical approach to building the transformation service |
| [mapping.md](mapping.md) | Field-level mapping from ORUK to Schema.org |

## Goals

1. Understand the landscape of standards related to Open Referral UK.
2. Design a lightweight transformation service that reads ORUK JSON feeds and produces consolidated Schema.org JSON-LD.
3. Plan future extensions for FHIR and MCP outputs.

## Next Steps

- Review the [standards overview](standards/README.md) to understand how HSDS, ORUK, Schema.org, and FHIR relate.
- Read the [technical approach](approach.md) for the proposed service architecture.
- Consult the [field mapping](mapping.md) for the ORUK → Schema.org transformation rules.

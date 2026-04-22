# Standards Reference

This directory contains descriptions of the key standards relevant to the ORUK Alternative Representations project.

## Standards

| Standard | File | Summary |
|----------|------|---------|
| HSDS – Human Services Data Specification | [hsds.md](hsds.md) | The international parent standard for service directories. |
| Open Referral UK (ORUK) | [open-referral-uk.md](open-referral-uk.md) | The UK national profile of HSDS, validated by iStandUK. |
| Schema.org | [schema-org.md](schema-org.md) | Web-structured-data vocabulary used by search engines and AI agents. |
| HL7 FHIR HealthcareService | [hl7-fhir.md](hl7-fhir.md) | Clinical interoperability standard used across NHS England. |

---

## How the Standards Relate

```mermaid
graph TD
    HSDS["HSDS\n(Human Services Data Specification)\nInternational parent standard"]
    ORUK["Open Referral UK (ORUK)\nUK national profile of HSDS\nValidated by iStandUK"]
    SCHEMA["Schema.org\nWeb structured-data vocabulary\n(JSON-LD / Microdata / RDFa)"]
    FHIR["HL7 FHIR R4\nHealthcareService resource\nClinical interoperability standard"]

    HSDS -->|"Profile / extension"| ORUK
    ORUK -->|"Transform / map"| SCHEMA
    ORUK -->|"Transform / map"| FHIR
    SCHEMA -->|"Consumed by"| SE["Search Engines\n& AI Agents"]
    FHIR -->|"Consumed by"| NHS["NHS / Clinical Systems\n(EPR, referral tools)"]
    ORUK -->|"Consumed by"| SD["UK Service Finder\nApplications"]
```

## Use-Case Fit

| Use Case | Best Standard |
|----------|---------------|
| UK local-authority service finder | **ORUK** |
| Search-engine / AI discoverability | **Schema.org** |
| NHS referral / social prescribing integration | **HL7 FHIR** |
| Cross-border / international exchange | **HSDS** |
| Combined web + AI + clinical pipeline | **ORUK → Schema.org + FHIR** |

---

## Standard Maturity and Adoption

```mermaid
quadrantChart
    title Maturity vs UK Adoption
    x-axis Low UK Adoption --> High UK Adoption
    y-axis Early Stage --> Mature
    HSDS: [0.35, 0.65]
    ORUK: [0.75, 0.55]
    Schema.org: [0.85, 0.95]
    HL7 FHIR: [0.65, 0.85]
```

---

## Further Reading

- [Plan overview](../README.md)
- [Technical approach](../approach.md)
- [ORUK → Schema.org field mapping](../mapping.md)

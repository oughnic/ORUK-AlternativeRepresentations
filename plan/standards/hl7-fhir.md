# HL7 FHIR – HealthcareService Resource

## Overview

[HL7 FHIR (Fast Healthcare Interoperability Resources)](https://hl7.org/fhir/) is the international standard for exchanging healthcare information electronically, maintained by Health Level Seven International (HL7).  FHIR represents clinical and administrative data as discrete **Resources** accessed via a RESTful API.

This document focuses on the **`HealthcareService`** resource, which is the FHIR equivalent of an ORUK `Service` and is directly relevant to mapping Open Referral UK data into healthcare information systems.

## HealthcareService Resource

### Purpose

`HealthcareService` describes the details of a healthcare service available at a location.  It is used by:

- NHS England to publish provider directories.
- Care Quality Commission (CQC) integrations.
- Integrated Care Board (ICB) referral systems.
- NHS 111 service directories.

### Key Elements

| Element | Cardinality | Description |
|---------|-------------|-------------|
| `id` | 1..1 | Logical resource identifier. |
| `active` | 0..1 | Whether the record is in active use. |
| `providedBy` | 0..1 | Reference to the providing `Organization`. |
| `category` | 0..* | Broad category of service (e.g. `SNOMED 408443003` – General Medical Practice). |
| `type` | 0..* | Specific type(s) of service (coded). |
| `specialty` | 0..* | Clinical specialty (e.g. `394814009` – General Practice). |
| `location` | 0..* | Reference(s) to `Location` resources. |
| `name` | 0..1 | Human-readable name of the service. |
| `comment` | 0..1 | Additional description. |
| `extraDetails` | 0..1 | Markdown description for additional context. |
| `telecom` | 0..* | Contact details (phone, email, URL). |
| `coverageArea` | 0..* | Reference to `Location` resources describing the service area. |
| `eligibility` | 0..* | Eligibility criteria (code + comment). |
| `program` | 0..* | Named programme the service is part of. |
| `characteristic` | 0..* | Collection of characteristics (accessibility, language, etc.). |
| `communication` | 0..* | Languages spoken at the service. |
| `referralMethod` | 0..* | How a patient/professional can refer (fax, phone, elec, etc.). |
| `appointmentRequired` | 0..1 | Whether an appointment is needed. |
| `availableTime` | 0..* | Opening hours (days of week + time range). |
| `notAvailable` | 0..* | Known closure periods. |
| `availabilityExceptions` | 0..1 | Free-text description of exceptional availability. |
| `endpoint` | 0..* | Technical endpoints for the service (e.g. FHIR endpoint URLs). |

### Related Resources

```
HealthcareService
  ├── providedBy ──> Organization
  ├── location   ──> Location
  │                    └── address : Address
  │                    └── position : (latitude, longitude)
  ├── coverageArea ──> Location
  └── endpoint   ──> Endpoint
```

## FHIR REST API

| Interaction | Description |
|-------------|-------------|
| `GET /HealthcareService` | Search / list all services. |
| `GET /HealthcareService/{id}` | Retrieve a single service. |
| `GET /HealthcareService?organization={id}` | Services by providing organisation. |
| `GET /HealthcareService?location.near=...` | Geospatial proximity search. |

Responses are FHIR `Bundle` resources (JSON or XML) with `entry` arrays of `HealthcareService` resources.

## UK-Specific Profiles

| Profile | Description |
|---------|-------------|
| **UKCore-HealthcareService** | NHS England / HL7 UK Core profile adding UK-specific terminology bindings. |
| **NHS Provider Directory** | Operational national directory using `HealthcareService` resources. |
| **NHSDigital-HealthcareService** | Specific extensions for NHS Digital (now NHS England) systems. |

## Mapping to ORUK

| ORUK entity/field | FHIR element |
|-------------------|--------------|
| `Service.name` | `HealthcareService.name` |
| `Service.description` | `HealthcareService.comment` / `extraDetails` |
| `Service.status` | `HealthcareService.active` |
| `Organization` | `Organization` resource (referenced by `providedBy`) |
| `Location` | `Location` resource (referenced by `location`) |
| `Schedule` | `HealthcareService.availableTime` |
| `Contact` (phone) | `HealthcareService.telecom` (system=phone) |
| `Contact` (email) | `HealthcareService.telecom` (system=email) |
| `Eligibility` | `HealthcareService.eligibility` |
| `ServiceArea` | `HealthcareService.coverageArea` |
| `TaxonomyTerm` | `HealthcareService.category` / `type` |
| `CostOption` | No direct mapping; use `HealthcareService.characteristic` or extension. |

## Strengths and Use Cases

| Strength | Detail |
|----------|--------|
| Clinical integration | Native support in NHS infrastructure (SMSP, GP systems, EPRs). |
| Terminology | First-class support for SNOMED CT, ICD-10, LOINC. |
| Mature standard | R4 and R4B widely deployed; R5 available. |
| Ecosystem | Extensive tooling: HAPI FHIR, Azure Health Data Services, Google Cloud Healthcare API. |

**Best suited to:** NHS referral pathways, social-prescribing link-worker tools, and any integration with clinical systems.

## Resources

- FHIR R4 HealthcareService: <https://hl7.org/fhir/R4/healthcareservice.html>
- HL7 UK Core: <https://simplifier.net/hl7fhirukcorer4>
- NHS England FHIR API: <https://digital.nhs.uk/developer/api-catalogue>
- HAPI FHIR server: <https://hapifhir.io/>

# Open Referral UK (ORUK)

## Overview

**Open Referral UK (ORUK)** is the UK national profile of the Human Services Data Specification (HSDS).  It is governed by [iStandUK](https://istanduk.org/) and is designed to enable local authorities, NHS bodies, and third-sector organisations across the United Kingdom to publish service-directory data in a consistent, machine-readable format.

ORUK extends HSDS with UK-specific requirements and is validated by the [iStandUK ORUK Validator](https://github.com/iStandUK/prod-oruk-validator), which publishes the normative JSON Schema.

## UK-Specific Extensions

| Extension | Detail |
|-----------|--------|
| **UPRN** | Unique Property Reference Number (Ordnance Survey) is required for `Location` records. |
| **USRN** | Unique Street Reference Number can optionally accompany a location. |
| **Taxonomy alignment** | Services should be categorised using the UK Adult Social Care Services (ASCS) taxonomy or equivalent local schemes. |
| **Service area** | `ServiceArea` records can reference ONS geography codes (LAD, LSOA, MSOA, etc.). |
| **Accreditation / review** | Optional fields for CQC registration numbers and last-review dates. |

## Data Model (ORUK-specific additions)

The ORUK model inherits all HSDS entities and adds:

```
Service ──< ServiceArea  (ONS geography codes)
Location ──  uprn        (required string)
           ──  usrn       (optional string)
Organization ── review   (last-reviewed date)
             ── accreditations (CQC, etc.)
```

## API Profile

ORUK mandates a RESTful JSON API following the HSDS API profile.  The iStandUK validator checks responses from:

- `GET /services`
- `GET /services/{id}`
- `GET /organizations`
- `GET /organizations/{id}`
- `GET /service_at_locations`
- `GET /taxonomy_terms`

The validator uses the JSON Schema published at <https://github.com/iStandUK/prod-oruk-validator> as the authoritative schema.

## Conformance Levels

| Level | Description |
|-------|-------------|
| **Bronze** | Minimal required fields populated; machine-readable. |
| **Silver** | Additional recommended fields including taxonomy terms. |
| **Gold** | Full coverage including schedules, eligibility, cost options, and regular reviews. |

## Governance and Adoption

- Maintained by iStandUK with input from MHCLG (Ministry of Housing, Communities & Local Government), NHSX, and local-authority practitioners.
- Adopted by dozens of UK local authorities and NHS Integrated Care Boards.
- Required by some DLUHC/MHCLG grant conditions for social-care data publication.

## Strengths and Use Cases

| Strength | Detail |
|----------|--------|
| UK legal compliance | Aligns with UK Government data standards and UPRN policy. |
| Validated | Machine-checkable conformance via the iStandUK validator. |
| Sector support | Active community of UK local authorities and NHS organisations. |

**Best suited to:** UK local-authority service directories, NHS social-prescribing platforms, and referral tools operating within the UK.

## Resources

- iStandUK: <https://istanduk.org/>
- ORUK Validator: <https://github.com/iStandUK/prod-oruk-validator>
- OpenReferralUK guidance: <https://openreferraluk.org/>
- HSDS parent standard: [hsds.md](hsds.md)

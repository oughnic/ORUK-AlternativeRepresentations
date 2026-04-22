# Human Services Data Specification (HSDS)

## Overview

The **Human Services Data Specification (HSDS)** – also known as *Open Referral* – is an open data standard that defines how information about health, human, and social services should be structured, stored, and shared.  It is maintained by the [Open Referral Initiative](https://openreferral.org/) and is currently at version 3.x.

HSDS provides a common vocabulary for organisations that publish "service directory" data: who provides what services, where, to whom, and how to access them.

## Key Concepts

| Concept | Description |
|---------|-------------|
| **Organization** | A legal entity (charity, local authority, NHS trust, etc.) that delivers services. |
| **Service** | A discrete offer of assistance (food bank, counselling, housing advice, etc.) made by an organisation. |
| **ServiceAtLocation** | The relationship between a service and the physical or virtual place at which it is delivered. |
| **Location** | A physical address or geographic point. |
| **Contact** | Phone numbers, email addresses, and web URLs associated with an organisation, service, or location. |
| **Schedule** | Opening hours and availability windows. |
| **Eligibility** | Criteria a person must meet to access a service (age range, residency, referral, etc.). |
| **CostOption** | Pricing or fee information for a service. |
| **Taxonomy / TaxonomyTerm** | Hierarchical categorisation of services (e.g. SNOMED CT, UK ASCS). |

## Data Model

HSDS uses a **relational entity model** expressed in JSON (for APIs) and CSV (for bulk data exchange).  The core entities and their primary relationships are:

```
Organization ──< Service ──< ServiceAtLocation >── Location
                   │
                   ├──< Schedule
                   ├──< Contact
                   ├──< Eligibility
                   ├──< CostOption
                   └──< ServiceTaxonomy >── TaxonomyTerm
```

## API Profile

HSDS defines a RESTful HTTP API profile (the *Open Referral API*) with endpoints such as:

- `GET /services` – paginated list of services
- `GET /services/{id}` – single service with related data
- `GET /organizations` – list of organisations
- `GET /taxonomy_terms` – list of taxonomy terms

Responses are JSON objects.  The v3.x specification introduces JSON-LD context hints and improved pagination metadata.

## Relationship to Open Referral UK

Open Referral UK (ORUK) is a **UK-localised profile** of HSDS.  It inherits the core entity model but adds:

- UK-specific mandatory fields (e.g. UPRN for addresses).
- Alignment with the UK Government's Local Authority data standards.
- A curated API profile validated by the iStandUK validator.

## Strengths and Use Cases

| Strength | Detail |
|----------|--------|
| Interoperability | Adopted across the USA, UK, Australia, Canada, and elsewhere. |
| Open governance | Maintained as a community standard with public GitHub repositories. |
| Broad scope | Covers social care, health, housing, legal aid, and more. |
| API-first | Designed for machine-readable consumption by referral tools. |

**Best suited to:** building service-finder applications, referral management systems, and local-authority / NHS service directories.

## Resources

- Specification: <https://docs.openreferral.org/>
- GitHub: <https://github.com/openreferral/specification>
- HSDS v3.0 schema: <https://github.com/openreferral/specification/tree/master/schema>

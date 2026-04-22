# Schema.org – Service Directories

## Overview

[Schema.org](https://schema.org/) is a collaborative, community-driven vocabulary for structured data on the web, maintained by a consortium including Google, Microsoft, Yahoo, and Yandex.  It defines a hierarchy of types and properties expressed in JSON-LD, Microdata, or RDFa that search engines and AI agents use to understand page content.

This document focuses on the Schema.org types that are most relevant to **service directories** – the use case driving this repository.

## Relevant Schema.org Types

### Core types

| Type | URL | Relevance |
|------|-----|-----------|
| `LocalBusiness` | <https://schema.org/LocalBusiness> | Organisation delivering services at a physical location. |
| `GovernmentService` | <https://schema.org/GovernmentService> | A service offered by a government or public-sector body. |
| `Service` | <https://schema.org/Service> | Generic service offering (parent of `GovernmentService`). |
| `Organization` | <https://schema.org/Organization> | Legal entity providing services. |
| `Place` | <https://schema.org/Place> | A physical location. |
| `PostalAddress` | <https://schema.org/PostalAddress> | Structured address. |
| `OpeningHoursSpecification` | <https://schema.org/OpeningHoursSpecification> | When a service is available. |
| `ContactPoint` | <https://schema.org/ContactPoint> | Phone, email, or URL for a service or organisation. |
| `Offer` | <https://schema.org/Offer> | Pricing / access conditions. |
| `GeoCoordinates` | <https://schema.org/GeoCoordinates> | Latitude / longitude. |

### Pending / community extensions

| Type | URL | Relevance |
|------|-----|-----------|
| `SpecialAnnouncement` | <https://schema.org/SpecialAnnouncement> | Temporary changes (e.g. COVID closures). |
| `ServiceChannel` | <https://schema.org/ServiceChannel> | How a service can be accessed (phone, online, in-person). |
| `Audience` / `PeopleAudience` | <https://schema.org/PeopleAudience> | Eligibility / target audience. |

## JSON-LD Example

A minimal `GovernmentService` in JSON-LD:

```json
{
  "@context": "https://schema.org",
  "@type": "GovernmentService",
  "@id": "https://example.gov.uk/services/food-bank-east",
  "name": "East District Food Bank",
  "description": "Weekly food parcels for residents in food poverty.",
  "provider": {
    "@type": "Organization",
    "@id": "https://example.gov.uk/organisations/abc-charity",
    "name": "ABC Community Charity",
    "url": "https://abccharity.org.uk"
  },
  "areaServed": {
    "@type": "AdministrativeArea",
    "name": "East District"
  },
  "availableChannel": {
    "@type": "ServiceChannel",
    "serviceUrl": "https://abccharity.org.uk/foodbank",
    "servicePhone": {
      "@type": "ContactPoint",
      "telephone": "+44-1234-567890",
      "contactType": "customer support"
    }
  },
  "openingHoursSpecification": {
    "@type": "OpeningHoursSpecification",
    "dayOfWeek": "https://schema.org/Wednesday",
    "opens": "10:00",
    "closes": "14:00"
  },
  "offers": {
    "@type": "Offer",
    "price": "0",
    "priceCurrency": "GBP"
  }
}
```

## Mapping to ORUK

The mapping from ORUK fields to Schema.org properties is detailed in [`../mapping.md`](../mapping.md).  Key correspondences:

| ORUK entity/field | Schema.org type/property |
|-------------------|--------------------------|
| `Service` | `GovernmentService` or `Service` |
| `Organization` | `Organization` |
| `Location` | `Place` + `PostalAddress` + `GeoCoordinates` |
| `Schedule` | `OpeningHoursSpecification` |
| `Contact` | `ContactPoint` |
| `CostOption` | `Offer` |
| `Eligibility` | `audience` → `PeopleAudience` |
| `ServiceArea` | `areaServed` → `AdministrativeArea` |

## Aggregated JSON-LD Response

When consolidating multiple ORUK feeds, the response should be a JSON-LD document with an `@graph` array:

```json
{
  "@context": "https://schema.org",
  "@graph": [
    { "@type": "GovernmentService", ... },
    { "@type": "GovernmentService", ... }
  ]
}
```

## Benefits for This Project

| Benefit | Detail |
|---------|--------|
| Search engine visibility | Google's rich results support `LocalBusiness` and `GovernmentService`. |
| AI/LLM discoverability | Structured JSON-LD can be consumed by AI assistants and agents. |
| Linked data | `@id` URIs enable deduplication and graph traversal across datasets. |
| Broad adoption | Schema.org is the most widely deployed structured-data vocabulary on the web. |

## Resources

- Schema.org: <https://schema.org/>
- GovernmentService type: <https://schema.org/GovernmentService>
- JSON-LD specification: <https://www.w3.org/TR/json-ld11/>
- Google Rich Results: <https://developers.google.com/search/docs/appearance/structured-data/local-business>

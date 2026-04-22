# ORUK → Schema.org Field Mapping

This document defines how data elements from Open Referral UK (ORUK) feeds are mapped to Schema.org types and properties when producing JSON-LD output.

## Guiding Principles

- **Receive liberally:** If a source field is absent or null, omit the corresponding Schema.org property from the output rather than failing.
- **Supply conservatively:** Only emit properties with well-defined, non-null values.
- **Typed identifiers:** Every output node carries a stable `@id` URI so that nodes can be cross-referenced within the `@graph`.
- **Fidelity over completeness:** A partial but accurate representation is preferred over a padded but inaccurate one.

---

## 1. Service → GovernmentService

The ORUK `Service` entity maps to `schema:GovernmentService` (or `schema:Service` for non-government providers).

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `id` | `@id` | Constructed as `<baseUrl>/services/<id>` |
| `name` | `name` | Direct mapping. |
| `alternate_name` | `alternateName` | Direct mapping. |
| `description` | `description` | Direct mapping. |
| `url` | `url` | Direct mapping. |
| `email` | `email` | Direct mapping (also see ContactPoint). |
| `status` | `schema:serviceType` note | `active` → no special annotation; `inactive` → `schema:discontinued` pattern (no standard property; use `additionalProperty`). |
| `interpretation_services` | `availableLanguage` | Map to `Language` type if structured. |
| `application_process` | `termsOfService` | Free-text description of how to access the service. |
| `wait_time` | `additionalProperty` | Name: `waitTime`. |
| `fees_description` | `offers.description` | See CostOption mapping below. |
| `accreditations` | `hasCredential` | Map to `EducationalOccupationalCredential` where possible. |
| `eligibility_description` | `audience.description` | See Eligibility mapping. |
| (organisation reference) | `provider` | `@id` reference to `Organization` node. |
| (location reference) | `location` | `@id` reference to `Place` node. |
| (schedule reference) | `openingHoursSpecification` | See Schedule mapping. |
| (contact reference) | `contactPoint` | See Contact mapping. |
| (taxonomy terms) | `additionalType` | Schema.org URL or keyword for service category. |
| (service area) | `areaServed` | See ServiceArea mapping. |
| (cost options) | `offers` | See CostOption mapping. |

---

## 2. Organization → Organization

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `id` | `@id` | Constructed as `<baseUrl>/organisations/<id>` |
| `name` | `name` | Direct mapping. |
| `alternate_name` | `alternateName` | Direct mapping. |
| `description` | `description` | Direct mapping. |
| `email` | `email` | Direct mapping. |
| `url` | `url` | Direct mapping. |
| `legal_status` | `legalName` / `additionalProperty` | Free text; use `additionalProperty` if no standard property applies. |
| `logo` | `logo` | URL string → `ImageObject`. |
| `uri` | `sameAs` | Linked-data URI for the organisation. |
| `year_incorporated` | `foundingDate` | Year only. |

---

## 3. Location → Place

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `id` | `@id` | Constructed as `<baseUrl>/locations/<id>` |
| `name` | `name` | Direct mapping. |
| `description` | `description` | Direct mapping. |
| `latitude` | `geo.latitude` | Nested in `GeoCoordinates`. |
| `longitude` | `geo.longitude` | Nested in `GeoCoordinates`. |
| `address.address_1` | `address.streetAddress` | Nested in `PostalAddress`. |
| `address.city` | `address.addressLocality` | Nested in `PostalAddress`. |
| `address.state_province` | `address.addressRegion` | Nested in `PostalAddress`. |
| `address.postal_code` | `address.postalCode` | Nested in `PostalAddress`. |
| `address.country` | `address.addressCountry` | ISO 3166-1 alpha-2 code (e.g. `GB`). |
| `location_type` | `additionalProperty` | Name: `locationType`. |
| `accessibility` | `amenityFeature` | Map each accessibility feature to `LocationFeatureSpecification`. |
| `url` | `url` | Venue URL if available. |
| `external_identifier` (UPRN) | `identifier` | `PropertyValue` with `propertyID: "UPRN"`. |

---

## 4. Schedule → OpeningHoursSpecification

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `weekday` | `dayOfWeek` | Map to Schema.org day URI (e.g. `https://schema.org/Monday`). |
| `opens_at` | `opens` | ISO 8601 time string (`HH:mm`). |
| `closes_at` | `closes` | ISO 8601 time string (`HH:mm`). |
| `valid_from` | `validFrom` | ISO 8601 date. |
| `valid_to` | `validThrough` | ISO 8601 date. |
| `description` | `description` | Free-text schedule description. |

**Day of week mapping:**

| ORUK value | Schema.org URI |
|------------|----------------|
| `Monday` | `https://schema.org/Monday` |
| `Tuesday` | `https://schema.org/Tuesday` |
| `Wednesday` | `https://schema.org/Wednesday` |
| `Thursday` | `https://schema.org/Thursday` |
| `Friday` | `https://schema.org/Friday` |
| `Saturday` | `https://schema.org/Saturday` |
| `Sunday` | `https://schema.org/Sunday` |
| `PublicHolidays` | `https://schema.org/PublicHolidays` |

---

## 5. Contact → ContactPoint

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `name` | `contactType` | Role label (e.g. "enquiries", "referrals"). |
| `phone` (number) | `telephone` | Include country code where possible. |
| `email` | `email` | Direct mapping. |
| `url` | `url` | Direct mapping. |
| `title` | `name` | Title of the contact (if person). |
| `department` | `contactType` | Department name if `name` is absent. |

---

## 6. Eligibility → Audience

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `eligibility_description` | `audience.description` | Free-text description of who can access the service. |
| `minimum_age` | `audience.suggestedMinAge` | Integer years. |
| `maximum_age` | `audience.suggestedMaxAge` | Integer years. |
| (audience type) | `audience.@type` | Use `PeopleAudience` for age-range criteria; `Audience` otherwise. |

---

## 7. CostOption → Offer

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `option` / `amount` | `price` | Numeric value. |
| `currency` | `priceCurrency` | ISO 4217 code (default `GBP`). |
| `amount_description` | `description` | Free-text cost description. |
| `valid_from` | `priceValidUntil` | Reuse if a `valid_to` is absent. |
| `valid_to` | `priceValidUntil` | ISO 8601 date. |
| `option` = "free" | `price: 0` | Normalise free services to `price: 0`. |

---

## 8. ServiceArea → areaServed

| ORUK field | Schema.org type/property | Notes |
|------------|--------------------------|-------|
| `service_area` (name) | `areaServed.name` | Name of the area. |
| `extent` (geographic code) | `areaServed.identifier` | ONS code (LAD, LSOA, etc.) as `PropertyValue`. |
| `extent_type` | `areaServed.additionalProperty` | Name: `extentType` (e.g. `LocalAuthorityDistrict`). |

---

## 9. TaxonomyTerm → additionalType / keywords

| ORUK field | Schema.org property | Notes |
|------------|---------------------|-------|
| `name` | `keywords` | Free-text taxonomy term name. |
| `code` (SNOMED, etc.) | `additionalType` | URL or `DefinedTerm` if a resolvable URI exists. |
| `taxonomy_id` | `additionalProperty` | Name: `taxonomyId`. |

---

## 10. Fields With No Direct Schema.org Mapping

These ORUK fields do not have direct Schema.org equivalents.  They should be included as `additionalProperty` values (`PropertyValue` nodes) to preserve fidelity.

| ORUK field | Suggested `additionalProperty` name |
|------------|-------------------------------------|
| `service.status` | `orukStatus` |
| `service.assured_date` | `assuredDate` |
| `service.assured_by` | `assuredBy` |
| `organization.legal_status` | `legalStatus` |
| `location.uprn` | `uprn` |
| `location.usrn` | `usrn` |
| `location.location_type` | `locationType` |

---

## Consolidated JSON-LD Structure

```json
{
  "@context": "https://schema.org",
  "@graph": [
    {
      "@type": "GovernmentService",
      "@id": "<baseUrl>/services/<service-id>",
      "name": "Service Name",
      "provider": { "@id": "<baseUrl>/organisations/<org-id>" },
      "location": { "@id": "<baseUrl>/locations/<location-id>" },
      "openingHoursSpecification": [...],
      "contactPoint": [...],
      "audience": {...},
      "offers": [...],
      "areaServed": [...],
      "keywords": "..."
    },
    {
      "@type": "Organization",
      "@id": "<baseUrl>/organisations/<org-id>",
      "name": "Organisation Name"
    },
    {
      "@type": "Place",
      "@id": "<baseUrl>/locations/<location-id>",
      "name": "Location Name",
      "address": { "@type": "PostalAddress", ... },
      "geo": { "@type": "GeoCoordinates", ... }
    }
  ]
}
```

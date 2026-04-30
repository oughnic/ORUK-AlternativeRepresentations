using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// An offer to provide a product or service.
/// Maps from an ORUK <c>CostOption</c> entity.
///
/// <para>
/// <b>Required properties (Google Structured Data):</b>
/// <list type="bullet">
///   <item>
///     <see cref="Price"/> — use <c>0</c> for free services
///     (maps from ORUK <c>option = "free"</c> or <c>amount = 0</c>).
///   </item>
///   <item>
///     <see cref="PriceCurrency"/> — ISO 4217 currency code (e.g. <c>"GBP"</c>).
///   </item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Offer"/>
/// </remarks>
public record SchemaOrgOffer
{
    /// <summary>Schema.org type discriminator — always <c>"Offer"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "Offer";

    /// <summary>
    /// The offer price.  Use <c>0</c> for free services.
    /// Maps from ORUK <c>CostOption.amount</c>.
    /// <b>Required</b> by Google Structured Data guidelines.
    /// </summary>
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    /// <summary>
    /// The ISO 4217 currency code for the price (e.g. <c>"GBP"</c>).
    /// Maps from ORUK <c>CostOption.currency</c>; defaults to <c>"GBP"</c> when absent.
    /// <b>Required</b> by Google Structured Data guidelines.
    /// </summary>
    [JsonPropertyName("priceCurrency")]
    public required string PriceCurrency { get; init; }

    /// <summary>
    /// A free-text description of this offer (e.g. <c>"Adult"</c>, <c>"Concession"</c>).
    /// Maps from ORUK <c>CostOption.amount_description</c>.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// The date after which the price is no longer valid.
    /// Maps from ORUK <c>CostOption.valid_to</c> (ISO 8601 date).
    /// </summary>
    [JsonPropertyName("priceValidUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PriceValidUntil { get; init; }

    /// <summary>
    /// The availability of the offer.
    /// Use Schema.org ItemAvailability URI constants such as
    /// <c>"https://schema.org/InStock"</c> or <c>"https://schema.org/LimitedAvailability"</c>.
    /// </summary>
    [JsonPropertyName("availability")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Availability { get; init; }
}

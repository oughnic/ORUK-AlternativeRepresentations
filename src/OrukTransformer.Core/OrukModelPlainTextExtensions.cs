using OrukModels.Models;

namespace OrukTransformer.Core;

/// <summary>
/// Plain-text convenience accessors for ORUK model properties that can contain HTML.
/// </summary>
public static class OrukModelPlainTextExtensions
{
    public static string? DescriptionPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.Description);

    public static string? AlertPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.Alert);

    public static string? FeesDescriptionPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.FeesDescription);

    public static string? ApplicationProcessPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.ApplicationProcess);

    public static string? InterpretationServicesPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.InterpretationServices);

    public static string? EligibilityDescriptionPlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.EligibilityDescription);

    public static string? WaitTimePlain(this OrukService service) =>
        OrukPlainText.ToPlainText(service.WaitTime);

    public static string? DescriptionPlain(this OrukOrganization organization) =>
        OrukPlainText.ToPlainText(organization.Description);

    public static string? DescriptionPlain(this OrukSchedule schedule) =>
        OrukPlainText.ToPlainText(schedule.Description);

    public static string? AttendingTypePlain(this OrukSchedule schedule) =>
        OrukPlainText.ToPlainText(schedule.AttendingType);

    public static string? NotesPlain(this OrukSchedule schedule) =>
        OrukPlainText.ToPlainText(schedule.Notes);

    public static string? DescriptionPlain(this OrukEligibility eligibility) =>
        OrukPlainText.ToPlainText(eligibility.Description);

    public static string? DescriptionPlain(this OrukAccessibility accessibility) =>
        OrukPlainText.ToPlainText(accessibility.Description);

    public static string? OptionPlain(this OrukCostOption costOption) =>
        OrukPlainText.ToPlainText(costOption.Option);

    public static string? AmountDescriptionPlain(this OrukCostOption costOption) =>
        OrukPlainText.ToPlainText(costOption.AmountDescription);

    public static string? DocumentPlain(this OrukRequiredDocument requiredDocument) =>
        OrukPlainText.ToPlainText(requiredDocument.Document);

    public static string? DescriptionPlain(this OrukTaxonomyTerm taxonomyTerm) =>
        OrukPlainText.ToPlainText(taxonomyTerm.Description);

    public static string? SourcePlain(this OrukFunding funding) =>
        OrukPlainText.ToPlainText(funding.Source);
}

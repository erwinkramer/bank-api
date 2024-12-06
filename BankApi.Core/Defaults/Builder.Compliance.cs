using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;

public static partial class ApiBuilder
{
    public static IHostApplicationBuilder AddComplianceServices(this IHostApplicationBuilder builder)
    {
        // Enable redaction of `[LogProperties]` objects
        builder.Logging.EnableRedaction();

        // Add the redaction services
        builder.Services.AddRedaction(builder =>
        {
            builder.SetRedactor<ErasingRedactor>(
                new DataClassificationSet([BankTaxonomy.ConfidentialData, BankTaxonomy.RestrictedData]));

            builder.SetRedactor<NullRedactor>(
                new DataClassificationSet(BankTaxonomy.UnrestrictedData)
            );
        });

        return builder;
    }
}
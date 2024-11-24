using Microsoft.Extensions.Compliance.Classification;

public static class BankTaxonomy
{
    public static string TaxonomyName => typeof(BankTaxonomy).FullName!;

    public static DataClassification UnrestrictedData => new(TaxonomyName, nameof(UnrestrictedData));

    public static DataClassification RestrictedData => new(TaxonomyName, nameof(RestrictedData));

    public static DataClassification ConfidentialData => new(TaxonomyName, nameof(ConfidentialData));
}

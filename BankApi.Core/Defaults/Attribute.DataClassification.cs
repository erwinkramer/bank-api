using Microsoft.Extensions.Compliance.Classification;

public class UnrestrictedDataAttribute : DataClassificationAttribute
{
    public UnrestrictedDataAttribute() : base(BankTaxonomy.UnrestrictedData) { }
}

public class RestrictedDataAttribute : DataClassificationAttribute
{
    public RestrictedDataAttribute() : base(BankTaxonomy.RestrictedData) { }
}

public class ConfidentialDataAttribute : DataClassificationAttribute
{
    public ConfidentialDataAttribute() : base(BankTaxonomy.ConfidentialData) { }
}

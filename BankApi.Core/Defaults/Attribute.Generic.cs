using System.ComponentModel.DataAnnotations;

public class GenericRangeAttribute : RangeAttribute
{
    public GenericRangeAttribute()
        : base(GlobalConfiguration.ApiSettings!.GenericBoundaries.Minimum, GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum)
    { }
}

public class GenericMaxLengthAttribute : MaxLengthAttribute
{
    public GenericMaxLengthAttribute()
        : base(GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum)
    { }
}

public class GenericRegularExpressionAttribute : RegularExpressionAttribute
{
    public GenericRegularExpressionAttribute()
        : base(GlobalConfiguration.ApiSettings!.GenericBoundaries.Regex)
    { }
}

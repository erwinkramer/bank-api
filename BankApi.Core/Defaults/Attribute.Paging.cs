using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class PageDefaultValueAttribute : DefaultValueAttribute
{
    public PageDefaultValueAttribute()
        : base(1)
    { }
}

public class PageRangeAttribute : RangeAttribute
{
    public PageRangeAttribute()
        : base(1, GlobalConfiguration.ApiSettings!.GenericBoundaries.Maximum)
    { }
}

public class PageSizeDefaultValueAttribute : DefaultValueAttribute
{
    public PageSizeDefaultValueAttribute()
        : base(GlobalConfiguration.ApiSettings!.PageSize.Default)
    { }
}

public class PageSizeRangeAttribute : RangeAttribute
{
    public PageSizeRangeAttribute()
        : base(GlobalConfiguration.ApiSettings!.PageSize.Minimum, GlobalConfiguration.ApiSettings!.PageSize.Maximum)
    { }
}

using System.ComponentModel.DataAnnotations;

public class PageSizeRangeAttribute : RangeAttribute
{
    public PageSizeRangeAttribute()
        : base(GlobalConfiguration.ApiSettings!.PageSize.Minimum, GlobalConfiguration.ApiSettings!.PageSize.Maximum)
    { }
}

using System.ComponentModel;

public class PageSizeDefaultValueAttribute : DefaultValueAttribute
{
    public PageSizeDefaultValueAttribute()
        : base(GlobalConfiguration.ApiSettings!.PageSize.Default)
    { }
}

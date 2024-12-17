using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TellerReport
{
    [Description("Name of the report.")]
    [RegularExpression(".*")]
    [MaxLength(Int32.MaxValue)]
    public string? Name { get; set; }
}

public class TellerReportList
{
    [MaxLength(Int32.MaxValue)]
    public List<TellerReport> data { get; set; } = new();

    [Range(0, Int32.MaxValue)]
    public int count { get; set; } = 0;
}

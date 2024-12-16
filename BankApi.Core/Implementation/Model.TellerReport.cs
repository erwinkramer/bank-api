using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TellerReport
{
    [Description("Name of the report.")]
    [RegularExpression(".*")]
    [MaxLength(7070)]
    public string? Name { get; set; }
}

public class TellerReportList
{
    [MaxLength(1010)]
    public List<TellerReport> data { get; set; } = new();

    [Range(0, int.MaxValue)]
    public int count { get; set; } = 0;
}
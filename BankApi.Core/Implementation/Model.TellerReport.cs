using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TellerReport
{
    [Description("Name of the report.")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string? Name { get; set; }
}

public class TellerReportList
{
    [GenericMaxLength]
    public List<TellerReport> data { get; set; } = new();

    [GenericRange]
    public int count { get; set; } = 0;
}

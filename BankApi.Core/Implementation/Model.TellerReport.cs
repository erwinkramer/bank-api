using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class TellerReport
{
    [Description("Name of the report.")]
    [RegularExpression(".*")]
    [MaxLength(7070)]
    public string? Name { get; set; }
}

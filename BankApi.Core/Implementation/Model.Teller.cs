using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Teller
{
    [MaxLength(Int32.MaxValue)]
    [Description("GitHub profile of the teller.")]
    public Uri? GitHubProfile { get; set; }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Teller
{
    [MaxLength(707070)]
    [Description("GitHub profile of the teller.")]
    public Uri? GitHubProfile { get; set; }
}

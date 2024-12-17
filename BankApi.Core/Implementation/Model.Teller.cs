using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Teller
{
    [MaxLength(36)]
    [Description("GitHub profile of the teller.")]
    public Uri? GitHubProfile { get; set; }
}

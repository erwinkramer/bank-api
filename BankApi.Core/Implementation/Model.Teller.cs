using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Teller
{
    [GenericMaxLength]
    [Description("GitHub profile of the teller.")]
    public Uri? GitHubProfile { get; set; }
}

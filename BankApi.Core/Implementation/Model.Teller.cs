using System.ComponentModel;

public class Teller
{
    [GenericMaxLength]
    [Description("GitHub profile of the teller.")]
    public Uri? GitHubProfile { get; set; }
}

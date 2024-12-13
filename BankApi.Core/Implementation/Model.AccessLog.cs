using System.ComponentModel;

public class AccessLogModel
{
    [RestrictedData]
    [Description("User Id of the requestor.")]
    public string? UserId { get; set; }

    [Description("Authentication type during access.")]
    public string? AuthenticationType { get; set; }
}

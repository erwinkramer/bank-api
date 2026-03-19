using CloudNative.CloudEvents;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public record BankEventData
{
    [GenericRegularExpression]
    [GenericMaxLength]
    public Guid? BankId { get; init; }
}

public class BankEvent
{
    [JsonIgnore]
    public CloudEvent CloudEvent { get; }

    public BankEvent(string subtype, DateTimeOffset? time)
    {
        CloudEvent = new CloudEvent();
        Id = Guid.NewGuid().ToString();
        Time = time;
        Source = new Uri("https://github.com/erwinkramer/bank-api");
        Type = "nl.banks." + subtype;
    }

    [JsonPropertyName("specversion")]
    [Required]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string SpecVersion { get; init; } = "1.0";

    [JsonPropertyName("id")]
    [Required]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string? Id
    {
        get => CloudEvent.Id;
        set => CloudEvent.Id = value;
    }

    [JsonPropertyName("source")]
    [Required]
    [GenericRegularExpression]
    [GenericMaxLength]
    public Uri? Source
    {
        get => CloudEvent.Source;
        set => CloudEvent.Source = value;
    }

    /// <summary>
    /// MUST be Reverse domain name notation
    /// https://logius-standaarden.github.io/NL-GOV-profile-for-CloudEvents/#type
    /// </summary>
    [JsonPropertyName("type")]
    [Required]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string? Type
    {
        get => CloudEvent.Type;
        set => CloudEvent.Type = value;
    }

    [JsonPropertyName("subject")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string? Subject
    {
        get => CloudEvent.Subject;
        set => CloudEvent.Subject = value;
    }

    [JsonPropertyName("time")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public DateTimeOffset? Time
    {
        get => CloudEvent.Time;
        set => CloudEvent.Time = value;
    }

    [JsonPropertyName("datacontenttype")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public string? DataContentType
    {
        get => CloudEvent.DataContentType;
        set => CloudEvent.DataContentType = value;
    }

    [JsonPropertyName("dataschema")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public Uri? DataSchema
    {
        get => CloudEvent.DataSchema;
        set => CloudEvent.DataSchema = value;
    }

    [JsonPropertyName("data")]
    public BankEventData? Data
    {
        get => CloudEvent.Data as BankEventData;
        set => CloudEvent.Data = value;
    }

    [JsonPropertyName("data_base64")]
    [GenericRegularExpression]
    [GenericMaxLength]
    public byte[]? DataBase64
    {
        get => CloudEvent.Data as byte[];
        set => CloudEvent.Data = value;
    }
}

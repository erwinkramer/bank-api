using System.ComponentModel;

/// <summary>
/// Attribute used to describe the ID of a bank.
/// Inherits from DescriptionAttribute to provide a default description.
/// </summary>
public class BankAttribute : DescriptionAttribute
{
    public BankAttribute() : base("Id of the bank.") { }
}

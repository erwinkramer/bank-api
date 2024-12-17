using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Attribute used to enforce a maximum length of 36 for GUIDs.
/// Inherits from MaxLengthAttribute to set the maximum length for GUIDs to 36 characters.
/// </summary>
public class IdAttribute : MaxLengthAttribute
{
    public IdAttribute() : base(36) { }
}

/// <summary>
/// Attribute used to describe the ID of a bank.
/// Inherits from DescriptionAttribute to provide a default description.
/// </summary>
public class BankAttribute : DescriptionAttribute
{
    public BankAttribute() : base("Id of the bank.") { }
}

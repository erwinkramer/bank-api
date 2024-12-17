using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class IdSchemaAttribute : MaxLengthAttribute
{
    public IdSchemaAttribute() : base(36) { }
}

public class BankIdAttribute : DescriptionAttribute
{
    public BankIdAttribute() : base("Id of the bank.") { }
}

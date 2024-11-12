using Microsoft.AspNetCore.Http.HttpResults;

public class Test
{
    [Fact]
    public async Task CreateBankReturnsCreated()
    {
        // Arrange
        await using var context = new MockDb().CreateDbContext();

        // Act
        var result = await BankOperation.CreateBank(new BankModel()
        {
            BankTier = BankTier.A,
            Id = 123,
            IsCompliant = false
        }, context);

        //Assert
        Assert.IsType<Created<BankModel>>(result);
    }
}

using ATM;
using FakeItEasy;
using FluentAssertions;

namespace Atm.Test;

public class AuthenticationTests
{
    private readonly IAccountReadRepository _fakeRepo;
    private readonly Authentication _auth; //sut

    public AuthenticationTests()
    {
        _fakeRepo = A.Fake<IAccountReadRepository>();
        _auth = new Authentication(_fakeRepo);
    }

    [Fact]
    public void FindByAccountNumber_WhenAccountDoesNotExist_ReturnsNull()
    {
        // Arrange
        int accountNumber = 123456;
        A.CallTo(() => _fakeRepo.FindAccount(accountNumber)).Returns(null);
        // Act
        Customer? result = _auth.FindByAccountNumber(accountNumber);
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindByAccountNumber_WhenAccountExists_ReturnsCustomer()
    {
        // Arrange
        int accountNumber = 123452;
        var fakeAccount = new AccountData
        (
            CustomerId: 1,
            AccountId: 10,
            FirstName: "John",
            LastName: "Doe",
            AccountNumber: accountNumber,
            PinHash: new byte[] { 1, 2, 3, 4 },
            Balance: 5000m,
            MinimumBalance: 1000m,
            IsActive: true
        );
        A.CallTo(() => _fakeRepo.FindAccount(accountNumber)).Returns(fakeAccount);
        //Act
        Customer? result = _auth.FindByAccountNumber(accountNumber);

        //Assert
        result.Should().NotBeNull();
        result.ID.Should().Be(1);
    }
}

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

    private AccountData CreateFakeAccount(
        int customerId = 1,
        int accountId = 10,
        string firstName = "John",
        string lastName = "Doe",
        int accountNumber = 123456,
        byte[]? pinHash = null,
        bool isActive = true,
        decimal balance = 5000m)
    {
        return new AccountData
        (
            CustomerId: customerId,
            AccountId: accountId,
            FirstName: firstName,
            LastName: lastName,
            AccountNumber: accountNumber,
            PinHash: pinHash ?? new byte[] { 1, 2, 3, 4 },
            Balance: balance,
            MinimumBalance: 1000m,
            IsActive: isActive);

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
        var fakeAccount = CreateFakeAccount(accountNumber: accountNumber);
        A.CallTo(() => _fakeRepo.FindAccount(accountNumber)).Returns(fakeAccount);

        //Act
        Customer? result = _auth.FindByAccountNumber(accountNumber);

        //Assert
        result.Should().NotBeNull();
        result.ID.Should().Be(1);
    }

    [Fact]
    public void ExistingUser_WhenAccountNotFound_ReturnsNull()
    {
        //Arrange
        int accountNumber = 123456;
        A.CallTo(() => _fakeRepo.FindAccount(accountNumber)).Returns(null);

        //Act
        Customer? result = _auth.ExistingUser(accountNumber, "1234");

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExistingUser_WhenAccountInactive_ReturnsNull()
    {
        //Arrange
        AccountData account = CreateFakeAccount(isActive: false);
        A.CallTo(() => _fakeRepo.FindAccount(account.AccountNumber)).Returns(account);

        //Act
        Customer? result = _auth.ExistingUser(account.AccountNumber, "1234");

        //Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExistingUser_WhenPinIsValid_ReturnsCustomer()
    {
        //Arrange
        AccountData account = CreateFakeAccount(pinHash: PinHasher.HashPin("1234"));
        A.CallTo(() => _fakeRepo.FindAccount(account.AccountNumber)).Returns(account);

        //Act
        Customer? result = _auth.ExistingUser(account.AccountNumber, "1234");

        //Assert
        result.Should().NotBeNull();
        result!.ID.Should().Be(account.CustomerId);
        result.AccountNumber.Should().Be(account.AccountNumber);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public void NewUser_WhenCreated_ReturnsCustomerMatchingAccount()
    {
        //Arrange
        AccountData account = CreateFakeAccount();
        A.CallTo(() => _fakeRepo.FindAccount(A<int>._)).Returns(null);
        A.CallTo(() => _fakeRepo.CreateAccount(
            A<string>._,
            A<string>._,
            A<int>._,
            A<byte[]>._,
            A<decimal>._)).Returns(account);

        //Act
        Customer result = _auth.NewUser("John", "Doe", "1234", 5000m);

        //Assert
        result.ID.Should().Be(account.CustomerId);
        result.FirstName.Should().Be(account.FirstName);
        result.LastName.Should().Be(account.LastName);
        result.AccountNumber.Should().Be(account.AccountNumber);
        result.AccountBalance.Should().Be((double)account.Balance);
    }

    [Fact]
    public void NewUser_WhenCreated_PassesHashedPinToRepository()
    {
        //Arrange
        byte[]? capturePinHash = null;

        A.CallTo(() => _fakeRepo.FindAccount(A<int>._)).Returns(null);
        A.CallTo(() => _fakeRepo.CreateAccount(
            A<string>._,
            A<string>._,
            A<int>._,
            A<byte[]>._,
            A<decimal>._)).Invokes(call => capturePinHash = call.GetArgument<byte[]>(3))
            .Returns(CreateFakeAccount());

        //Act
        _auth.NewUser("John", "Doe", "1234", 0m);

        //Assert
        capturePinHash.Should().NotBeNull();
        capturePinHash.Length.Should().Be(48);
        PinHasher.VerifyPin("1234", capturePinHash).Should().BeTrue();
    }
}

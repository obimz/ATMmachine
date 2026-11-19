using ATM;
using FakeItEasy;
using FluentAssertions;
using static Atm.Test.FakeAccountHelper;


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
        var fakeAccount = Create(accountNumber: accountNumber);
        A.CallTo(() => _fakeRepo.FindAccount(accountNumber)).Returns(fakeAccount);

        //Act
        Customer? result = _auth.FindByAccountNumber(accountNumber);

        //Assert
        result.Should().NotBeNull();
        result!.ID.Should().Be(1);
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
        AccountData account = Create(isActive: false);
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
        AccountData account = Create(pinHash: PinHasher.HashPin("1234"));
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
        AccountData account = Create();
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
            .Returns(Create());

        //Act
        _auth.NewUser("John", "Doe", "1234", 0m);

        //Assert
        capturePinHash.Should().NotBeNull();
        capturePinHash.Length.Should().Be(48);
        PinHasher.VerifyPin("1234", capturePinHash).Should().BeTrue();
    }

    [Fact]
    public void NewUser_WhenAccountNumberIsTaken_RetriesWithAnotherNumber()
    {
        //Arrange
        A.CallTo(() => _fakeRepo.FindAccount(A<int>._)).ReturnsNextFromSequence(
            Create(accountNumber: 123456),
            null);
        A.CallTo(() => _fakeRepo.CreateAccount(
            A<string>._,
            A<string>._,
            A<int>._,
            A<byte[]>._,
            A<decimal>._)).Returns(Create());

        //Act
        _auth.NewUser("John", "Doe", "1234", 0m);

        //Assert
        A.CallTo(() => _fakeRepo.FindAccount(A<int>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _fakeRepo.CreateAccount(
            A<string>._,
            A<string>._,
            A<int>._,
            A<byte[]>._,
            A<decimal>._)).MustHaveHappenedOnceExactly();
    }
}

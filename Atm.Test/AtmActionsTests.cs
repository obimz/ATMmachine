using ATM;
using FakeItEasy;
using FluentAssertions;
using static Atm.Test.FakeAccountHelper;


namespace Atm.Test;

public class AtmActionsTests
{
    private readonly IAccountReadRepository _fakeRepo;
    private readonly ITransactionsCommandsRepo _fakeTransactRepo;
    
    private readonly ATMactions _action; //THE SUT (System Under Test)

    public AtmActionsTests()
    {
        _fakeRepo = A.Fake<IAccountReadRepository>();
        _fakeTransactRepo = A.Fake<ITransactionsCommandsRepo>();
        _action = new ATMactions(_fakeRepo, _fakeTransactRepo);
    }


    [Fact]
    public void Deposit_WhenAccountExists_UpdatesCustomerBalance()
    {
        //Arrange
        var customer = new Customer(1, "John", "Doe", 123456, 100.00);
        AccountData existingAccount = Create(accountNumber: 123456);

        AccountData accountAfterDeposit = Create(accountNumber: 123456, balance: 3000.00m);

        A.CallTo(() => _fakeRepo.FindAccount(123456)).Returns(existingAccount);
        A.CallTo(() => _fakeTransactRepo.ApplyDeposit(existingAccount.AccountId, 100.00m)).Returns(accountAfterDeposit);

        //Act
        _action.Deposit(customer, 100.00m);

        //Assert
        customer.AccountBalance.Should().Be(3000.00);
    }

    [Fact]
    public void Transfer_WhenSucessful_UpdatesSenderBalanceFromRepository()
    {
        //Arrange
        var sender = new Customer(2, "Samuel", "James", 232122, 1000.00);

        AccountData senderAccount = Create(accountNumber: 232122, accountId:10, balance: 1000.00m);
        AccountData receiverAccount = Create(accountNumber: 123456, accountId: 32);
        AccountData senderAfterTransfer = Create(accountNumber: 232122, balance: 300.00m);

        A.CallTo(() => _fakeRepo.FindAccount(232122))
            .ReturnsNextFromSequence(senderAccount, senderAfterTransfer);
        A.CallTo(() => _fakeRepo.FindAccount(123456)).Returns(receiverAccount);

        //Act
        _action.Transfer(sender, 123456, 700);

        //Assert
        sender.AccountBalance.Should().Be(300);
        A.CallTo(() => _fakeTransactRepo.Transfer(10, 32, 700)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Transfer_WhenReceiverDoesNotExist_DoesNotTransferOrChangeSenderBalance()
    {
        //Arrange
        var sender = new Customer(2, "Samuel", "James", 232122, 1000.00);

        AccountData senderAccount = Create(accountNumber: 232122, accountId: 10, balance: 1000.00m);
        
        A.CallTo(() => _fakeRepo.FindAccount(232122)).Returns(senderAccount);
        A.CallTo(() => _fakeRepo.FindAccount(123456)).Returns(null);

        //Act
        _action.Transfer(sender, 123456, 700);

        //Assert
        sender.AccountBalance.Should().Be(1000);
        A.CallTo(() => _fakeTransactRepo.Transfer(10, 32, 700)).MustNotHaveHappened();
    }
}

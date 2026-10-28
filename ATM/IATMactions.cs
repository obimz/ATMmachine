using System;

namespace ATM;

public interface IATMactions
{
    void ShowBalance(Customer customer);

    void Deposit(Customer customer);

    void Withdraw(Customer customer);

    void Transfer(Customer sender);

    void ShowTransactionsHistory();
}

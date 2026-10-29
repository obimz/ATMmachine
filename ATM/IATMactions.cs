namespace ATM
{
    public interface IATMactions
    {
        void ShowBalance(Customer customer);

        void Deposit(Customer customer, decimal amount);

        void Withdraw(Customer customer, decimal amount);

        void Transfer(Customer sender, int receiverAccountNumber, decimal amount);

        void ShowTransactionsHistory();
    }
}
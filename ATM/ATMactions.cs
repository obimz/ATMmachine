using Serilog;
using System;

namespace ATM
{
    public class ATMactions : IATMactions
    {
        private readonly IAccountReadRepository _databaseRead;
        private readonly ITransactionsCommandsRepo _databaseTransact;

        public static Customer? CurrentUser { get; private set; }

        public ATMactions(IAccountReadRepository databaseRead, ITransactionsCommandsRepo databaseTransact)
        {
            _databaseRead = databaseRead;
            _databaseTransact = databaseTransact;
        }

        public void ShowBalance(Customer customer)
        {
            Log.Information("Customer with account {AccountNumber} initialized ShowBalance", customer.AccountNumber);
            Console.WriteLine($"\nYour current balance is: {customer.AccountBalance}");
        }

        // Accepts 'amount' directly from ScreenDisplay
        public void Deposit(Customer customer, decimal amount)
        {
            Log.Information("Deposit requested for account {AccountNumber}, amount {Amount}", customer.AccountNumber, amount);

            try
            {
                AccountData? account = _databaseRead.FindAccount(customer.AccountNumber);
                if (account is null)
                {
                    Log.Warning("Deposit failed: account {AccountNumber} not found", customer.AccountNumber);
                    Console.WriteLine("Account could not be found.");
                    return;
                }

                AccountData updatedAccount = _databaseTransact.ApplyDeposit(account.AccountId, amount);
                customer.AccountBalance = (double)updatedAccount.Balance;

                Log.Information("Deposit successful for account {AccountNumber}, new balance {Balance}", customer.AccountNumber, customer.AccountBalance);
                ShowBalance(customer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deposit failed for account {AccountNumber}", customer.AccountNumber);
                Console.WriteLine($"Deposit failed: {ex.Message}");
            }
        }

        // Accepts 'amount' directly from ScreenDisplay
        public void Withdraw(Customer customer, decimal amount)
        {
            Log.Information("Withdrawal requested for account {AccountNumber}, amount {Amount}", customer.AccountNumber, amount);

            try
            {
                AccountData? account = _databaseRead.FindAccount(customer.AccountNumber);
                if (account is null)
                {
                    Log.Warning("Withdrawal failed: account {AccountNumber} not found", customer.AccountNumber);
                    Console.WriteLine("Account could not be found.");
                    return;
                }

                AccountData updatedAccount = _databaseTransact.ApplyWithdrawal(account.AccountId, amount);
                customer.AccountBalance = (double)updatedAccount.Balance;

                Log.Information("Withdrawal successful for account {AccountNumber}, new balance {Balance}", customer.AccountNumber, customer.AccountBalance);
                ShowBalance(customer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Withdrawal failed for account {AccountNumber}", customer.AccountNumber);
                Console.WriteLine($"Withdrawal failed: {ex.Message}");
            }
        }


        public void Transfer(Customer sender, int receiverAccountNumber, decimal amount)
        {
            AccountData? senderAccount = _databaseRead.FindAccount(sender.AccountNumber);
            AccountData? receiverAccount = _databaseRead.FindAccount(receiverAccountNumber);

            if (senderAccount is null || receiverAccount is null)
            {
                Log.Warning("Transfer failed: sender or receiver account not found");
                Console.WriteLine("No customer found with that account number. Transfer cancelled.");
                return;
            }

            Log.Information("Transfer requested from {SenderAccountNumber} to {ReceiverAccountNumber}, amount {Amount}", sender.AccountNumber, receiverAccountNumber, amount);

            try
            {
                _databaseTransact.Transfer(senderAccount.AccountId, receiverAccount.AccountId, amount);

                AccountData? updatedSender = _databaseRead.FindAccount(sender.AccountNumber);
                if (updatedSender is not null)
                {
                    sender.AccountBalance = (double)updatedSender.Balance;
                }

                Log.Information("Transfer successful from {SenderAccountNumber} to {ReceiverAccountNumber}, amount {Amount}", sender.AccountNumber, receiverAccountNumber, amount);
                ShowBalance(sender);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Transfer failed for account {SenderAccountNumber}", sender.AccountNumber);
                Console.WriteLine($"Transfer failed: {ex.Message}");
            }
        }

        public void ShowTransactionsHistory()
        {
            if (CurrentUser is null)
            {
                Log.Warning("ShowTransactionsHistory was called with no user");
                return;
            }

            AccountData? account = _databaseRead.FindAccount(CurrentUser.AccountNumber);
            if (account is null)
            {
                Console.WriteLine("Account could not be found.\n");
                return;
            }

            var transactions = _databaseTransact.GetTransactions(account.AccountId);
            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions recorded.\n");
                return;
            }

            foreach (TransactionData transaction in transactions)
            {
                Console.WriteLine(
                    $"Transaction: {transaction.TransactionType}\n" +
                    $"\tAmount: {transaction.Amount}\n" +
                    $"\tBalance after: {transaction.BalanceAfter}\n" +
                    $"\tDate: {transaction.TransactionDate}\n" +
                    $"\tDescription: {transaction.Description}\n");
            }
        }
    }
}
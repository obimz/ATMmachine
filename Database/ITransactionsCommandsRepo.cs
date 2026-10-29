using System;

namespace ATM
{
    public interface ITransactionsCommandsRepo
    {
        AccountData ApplyDeposit(int accountId, decimal ammount);

        AccountData ApplyWithdrawal(int accountId, decimal amount);

        void Transfer(int senderAccountId, int receiverAccountId, decimal Amount);

        List<TransactionData> GetTransactions(int accountId);
    }
}
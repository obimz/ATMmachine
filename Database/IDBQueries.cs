using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ATM
{
    public interface IDBQueries
    {
        AccountData? FindAccount(int accountNumber);

        AccountData CreateAccount(
            string firstName,
            string lastName,
            int accountNumber,
            byte[] pinHash,
            decimal initialBalance = 0);

        AccountData ApplyDeposit(int accountId, decimal ammount);

        AccountData ApplyWithdrawal(int accountId, decimal amount);

        void Transfer(int senderAccountId, int receiverAccountId, decimal Amount);

        List<TransactionData> GetTransactions(int accountId);
            
    }
}

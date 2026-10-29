using System;
using System.Collections.Generic;

namespace ATM
{
    public interface IAccountReadRepository
    {
        AccountData? FindAccount(int accountNumber);

        AccountData CreateAccount(
            string firstName,
            string lastName,
            int accountNumber,
            byte[] pinHash,
            decimal initialBalance = 0);       

    }
}

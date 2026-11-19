using ATM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atm.Test;

public static class FakeAccountHelper
{
    public static AccountData Create(
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
}

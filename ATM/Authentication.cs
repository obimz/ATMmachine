using Serilog;

namespace ATM
{
    public class Authentication
    {
        private readonly IAccountReadRepository _databaseRead;

        public Authentication(IAccountReadRepository databaseRead)
        {
            _databaseRead = databaseRead;
        }

        public Customer? ExistingUser(int accountNumber, string pin)
        {
            AccountData? account = _databaseRead.FindAccount(accountNumber);
            if (account is null || !account.IsActive)
            {
                Log.Warning("Login failed: account {Account Number} not found or is inactive", accountNumber);
                return null;
            }

            bool isValid = PinHasher.VerifyPin(pin, account.PinHash);
            if (isValid)
            {
                Log.Information("Login successful {AccountNumber}", accountNumber);
                return ToCustomer(account);
            }
            return null;
        }

        private int Generate6DigitAccountNumber()
        {
            int accountNumber;
            do
            {
                accountNumber = Random.Shared.Next(123459, 1000000); //This creates a random 6-digit account number between 123459 and 999999
            } while (_databaseRead.FindAccount(accountNumber) is not null);

            return accountNumber;
        }

        public Customer NewUser(
            string firstName,
            string lastName,
            string pin,
            decimal balance)
        {
            Log.Information("Creating new account for {firstName} {lastName}", firstName, lastName);

            int accountNumber = Generate6DigitAccountNumber();
            byte[] pinHash = PinHasher.HashPin(pin);
            AccountData account = _databaseRead.CreateAccount(
                firstName,
                lastName,
                accountNumber,
                pinHash,
                balance);

            Log.Information("Account created successfully {AccountNumber}", account.AccountNumber);
            return ToCustomer(account);
        }

        public Customer? FindByAccountNumber(int accountNumber)
        {
            AccountData? account = _databaseRead.FindAccount(accountNumber);
            return account is null ? null : ToCustomer(account);
        }

        private static Customer ToCustomer(AccountData account)
        {
            return new Customer(
                account.CustomerId,
                account.FirstName,
                account.LastName,
                account.AccountNumber,
                (double)account.Balance);
        }
    }
}
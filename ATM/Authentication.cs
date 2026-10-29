using Serilog;

namespace ATM
{
    public class Authentication
    {
        private static readonly IAccountReadRepository databaseRead = new DBQueries(); //create a database-query object with IAccountReadRepository as the field type

        public static Customer ExistingUser(int accountNumber, string pin)
        {
            AccountData? account = databaseRead.FindAccount(accountNumber);
            if (account is null || !account.IsActive)
            {
                Log.Warning("Login failed: account {Account Number} not found or is inactive", accountNumber);
                return null;
            }

            return PinHasher.VerifyPin(pin, account.PinHash)
                ? ToCustomer(account)
                : null;

            Log.Information("Login successful {AccountNumber}", accountNumber);
        }

        private static int Generate6DigitAccountNumber()
        {
            int accountNumber;
            do
            {
                accountNumber = Random.Shared.Next(123459, 1000000); //This creates a random 6-digit account number between 123459 and 999999
            } while (databaseRead.FindAccount(accountNumber) is not null);

            return accountNumber;
        }

        public static Customer NewUser(
            string firstName,
            string lastName,
            string pin,
            decimal balance)
        {
            Log.Information("Creating new account for {firstName} {lastName}", firstName, lastName);

            int accountNumber = Generate6DigitAccountNumber();
            byte[] pinHash = PinHasher.HashPin(pin);
            AccountData account = databaseRead.CreateAccount(
                firstName,
                lastName,
                accountNumber,
                pinHash,
                balance);

            Log.Information("Account created successfully {AccountNumber}", account.AccountNumber);
            return ToCustomer(account);
        }

        public static Customer FindByAccountNumber(int accountNumber)
        {
            AccountData? account = databaseRead.FindAccount(accountNumber);
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
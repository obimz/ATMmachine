using Serilog;

namespace ATM
{
    public class Authentication
    {
        public static Customer ExistingUser(int accountNumber, string pin)
        {
            AccountData? account = DBQueries.FindAccount(accountNumber);
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
                accountNumber = Random.Shared.Next(123459, 1000000);
            } while (DBQueries.FindAccount(accountNumber) is not null);

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
            AccountData account = DBQueries.CreateAccount(
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
            AccountData? account = DBQueries.FindAccount(accountNumber);
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
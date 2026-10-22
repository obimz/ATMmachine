using System;
using System.Linq;

namespace ATM
{
    public class ATMactions
    {
        public static Customer CurrentUser { get; private set; }

        public static void ShowBalance(Customer customer)
        {
            Console.WriteLine($"\nYour current balance is: {customer.AccountBalance}");
        }

        public static void Deposit(Customer customer)
        {
            Console.WriteLine("This is the section for depositing money. ");
            decimal amount = EnterAmount();

            try
            {
                AccountData? account = DBQueries.FindAccount(customer.AccountNumber);
                if (account is null)
                {
                    Console.WriteLine("Account could not be found.");
                    return;
                }

                AccountData updatedAccount = DBQueries.ApplyDeposit(account.AccountId, amount);
                customer.AccountBalance = (double)updatedAccount.Balance;
                ShowBalance(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deposit failed: {ex.Message}");
            }
        }

        public static void Withdraw(Customer customer)
        {
            Console.WriteLine("This is the section for withdrawing money. ");
            decimal amount = EnterAmount();

            try
            {
                AccountData? account = DBQueries.FindAccount(customer.AccountNumber);
                if (account is null)
                {
                    Console.WriteLine("Account could not be found.");
                    return;
                }

                AccountData updatedAccount = DBQueries.ApplyWithdrawal(account.AccountId, amount);
                customer.AccountBalance = (double)updatedAccount.Balance;
                ShowBalance(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Withdrawal failed: {ex.Message}");
            }
        }

        public static void Transfer(Customer sender)
        {
            Console.WriteLine("This is the section for transferring money. ");
            int recvAcc;
            do
            {
                Console.Write("Enter receiver account number: ");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out recvAcc))
                {
                    Console.WriteLine("Error: Invalid account number!");
                    recvAcc = 0;
                }
            } while (recvAcc == 0);

            AccountData? senderAccount = DBQueries.FindAccount(sender.AccountNumber);
            AccountData? receiverAccount = DBQueries.FindAccount(recvAcc);
            if (senderAccount is null || receiverAccount is null)
            {
                Console.WriteLine("No customer found with that account number. Transfer cancelled.");
                return;
            }

            decimal amount = EnterAmount();
            try
            {
                DBQueries.Transfer(
                    senderAccount.AccountId,
                    receiverAccount.AccountId,
                    amount);

                AccountData? updatedSender = DBQueries.FindAccount(sender.AccountNumber);
                if (updatedSender is not null)
                {
                    sender.AccountBalance = (double)updatedSender.Balance;
                }

                ShowBalance(sender);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transfer failed and was rolled back: {ex.Message}");
            }
        }

        public static void ShowTransactionsHistory()
        {
            Console.WriteLine("\t\tTransactions made are:\n");
            if (CurrentUser is null)
            {
                return;
            }

            AccountData? account = DBQueries.FindAccount(CurrentUser.AccountNumber);
            if (account is null)
            {
                Console.WriteLine("Account could not be found.\n");
                return;
            }

            List<TransactionData> transactions = DBQueries.GetTransactions(account.AccountId);
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

        public static Customer Login()
        {
            while (true)
            {
                ScreenDisplay.OnboardingMessage();

                Console.Write("Are you an existing user? (y/n): ");
                var isExisting = Console.ReadLine()?.Trim().ToLower() == "y";

                if (isExisting)
                {
                    int accNum;
                    do
                    {
                        Console.Write("Enter your account number: ");
                        var input = Console.ReadLine();
                        if (!int.TryParse(input, out accNum))
                        {
                            accNum = 0;
                            Console.WriteLine("Error! Invalid account number!");
                        }
                    } while (accNum == 0);

                    string accPin;
                    do
                    {
                        Console.Write("Enter your 4-digit account pin: ");
                        accPin = Console.ReadLine()?.Trim() ?? "";
                        if (accPin.Length != 4 || !accPin.All(char.IsDigit))
                        {
                            accPin = "";
                            Console.WriteLine("Invalid account pin!");
                        }
                    } while (string.IsNullOrEmpty(accPin));

                    var customer = Authentication.ExistingUser(accNum, accPin);
                    if (customer != null)
                    {
                        CurrentUser = customer;
                        Console.Clear();
                        ScreenDisplay.WelcomeCustomerMsg(customer);
                        return customer;
                    }

                    Console.WriteLine("No matching customer found. Try again or create a new user.");
                    // loops and let user choose again
                    continue;
                }
                else
                {
                    string firstName;
                    do
                    {
                        Console.Write("Please enter your first name: ");
                        firstName = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(firstName) || !firstName.All(c => char.IsLetter(c)))
                        {
                            Console.WriteLine("Error: First name must contain only letters and cannot be empty!");
                            firstName = "";
                        }
                    } while (string.IsNullOrWhiteSpace(firstName));

                    string lastName;
                    do
                    {
                        Console.Write("Please enter your last name: ");
                        lastName = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(lastName) || !lastName.All(c => char.IsLetter(c)))
                        {
                            Console.WriteLine("Error: Last name must contain only letters and cannot be empty!");
                            lastName = "";
                        }
                    } while (string.IsNullOrWhiteSpace(lastName));

                    string pin;
                    do
                    {
                        Console.Write("Choose a PIN: ");
                        pin = Console.ReadLine()?.Trim() ?? "";
                        if (pin.Length != 4 || !pin.All(char.IsDigit))
                        {
                            Console.WriteLine("Error: Invalid PIN!");
                            pin = "";
                        }
                    } while (string.IsNullOrEmpty(pin));

                    decimal bal = 0;

                    var newCustomer = Authentication.NewUser(firstName, lastName, pin, bal);
                    CurrentUser = newCustomer;
                    Console.Clear();
                    Console.WriteLine($"\n\nYour account has been created successfully! Your account number is: {newCustomer.AccountNumber}");
                    ScreenDisplay.WelcomeCustomerMsg(newCustomer);
                    return newCustomer;
                }
            }
        }

        public static void Menu(Customer customer)
        {
            bool looping = true;

            while (looping)
            {
                ScreenDisplay.DisplayMenu(customer);
                int option = 0;
                do
                {
                    

                    Console.Write("\nChoose an option: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out option) || option < 1 || option > 6)
                    {
                        Console.WriteLine("Invalid input");
                        option = 0;
                    }

                } while (option == 0);

                switch (option)
                {
                    case 1:
                        ShowBalance(customer);
                        break;
                    case 2:
                        Deposit(customer);
                        break;
                    case 3:
                        Withdraw(customer);
                        break;
                    case 4:
                        Transfer(customer);
                        break;
                    case 5:
                        ShowTransactionsHistory();
                        break;
                    default:
                        looping = false;
                        break;
                }
            }
        }

        private static decimal EnterAmount()
        {
            decimal num;
            do
            {
                Console.Write("Enter the amount: ");
                var input = Console.ReadLine();
                if (!decimal.TryParse(input, out num) || num <= 0)
                {
                    Console.WriteLine("Error: Enter a positive numeric amount!");
                    num = 0;
                }
            } while (num == 0);
            return num;
        }

    }
}
using System;
using System.Collections.Generic;

namespace ATM
{
    public class ATMactions
    {
        private record TransactionEntry(Transactions Type, DateTime Time);
        private static readonly List<TransactionEntry> transactions = new();
        public static void ShowBalance(Customer customer)
        {
            Console.WriteLine($"\nYour current balance is: {customer.AccountBalance}");

            
        }
        public static void Deposit(Customer customer)
        {
            Console.WriteLine("This is the section for depositing money. ");
            float amt = EnterAmount();
            customer.UpdateBalance(amt, UpdateType.add);
            ShowBalance(customer);
        }
        public static void Withdraw(Customer customer)
        {
            Console.WriteLine("This is the section for withdrawing money. ");
            float amt = EnterAmount();
            customer.UpdateBalance(amt, UpdateType.subtract);
            ShowBalance(customer);
        }
        public static void Transfer(Customer sender)
        {
            Console.WriteLine("This is the section for transferring money. ");
            //recvAcc is the account number of the receiver
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

            var receiver = Authentication.FindByAccountNumber(recvAcc);
            if (receiver == null)
            {
                Console.WriteLine("No customer found with that account number. Transfer cancelled.");
                return;
            }

            float amt = EnterAmount();

            if (!sender.CanWithdraw(amt))
            {
                Console.WriteLine("Error: Insufficient funds for transfer.");
                return;
            }

            
            sender.UpdateBalance(amt, UpdateType.subtract);
            try
            {
                receiver.UpdateBalance(amt, UpdateType.add);
            }
            catch (Exception)
            {
                sender.UpdateBalance(amt, UpdateType.add);
                Console.WriteLine("Transfer failed and was rolled back.");
                return;
            }

            ShowBalance(sender);

        }

        public static void ShowTransactionsHistory()
        {
            Console.WriteLine("\t\tTransactions made are:\n");
            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions recorded.\n");
                return;
            }
            foreach (var t in transactions)
            {
                Console.WriteLine($"Transaction: {t.Type}\n\tDate: {t.Time}\n");
            }
        }

        public static Customer Login()
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

                int accPin;
                do
                {
                    Console.Write("Enter your account pin: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out accPin))
                    {
                        accPin = 0;
                        Console.WriteLine("Invalid account pin!");
                    }
                } while (accPin == 0);

                var customer = Authentication.ExistingUser(accNum, accPin);
                if (customer != null)
                {
                    Console.Clear();
                    ScreenDisplay.WelcomeCustomerMsg(customer);
                    return customer;
                }

                Console.WriteLine("No matching customer found. Try again or create a new user.");
                return Login();
            }
            else
            {
                Console.Write("First name: ");
                var first = Console.ReadLine();
                Console.Write("Last name: ");
                var last = Console.ReadLine();            

                int pin;
                do
                {
                    Console.Write("Choose a PIN: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out pin))
                    {
                        Console.WriteLine("Error: Invalid PIN!");
                        pin = 0;
                    }
                } while (pin == 0);

                double bal = 0;

                var newCustomer = Authentication.NewUser(first, last, pin, bal);
                Console.Clear();
                Console.WriteLine($"\n\nYour account has been created successfully! Your account number is: {newCustomer.AccountNumber}");
                ScreenDisplay.WelcomeCustomerMsg(newCustomer);
                return newCustomer;
            }
        }

        public static void Menu(Customer customer)
        {
            bool looping = true;

            while (looping)
            {
                ScreenDisplay.DisplayMenu();
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
                        transactions.Add(new TransactionEntry(Transactions.ShowBalance, DateTime.Now));
                        break;
                    case 2:
                        Deposit(customer);
                        transactions.Add(new TransactionEntry(Transactions.Deposit, DateTime.Now));
                        break;
                    case 3:
                        Withdraw(customer);
                        transactions.Add(new TransactionEntry(Transactions.Withdraw, DateTime.Now));
                        break;
                    case 4:
                        Transfer(customer);
                        transactions.Add(new TransactionEntry(Transactions.Transfer, DateTime.Now));
                        break;
                    case 5:
                        ShowTransactionsHistory();
                        transactions.Add(new TransactionEntry(Transactions.ShowTransactionsHistory, DateTime.Now));
                        break;
                    default:
                        looping = false;
                        break;
                }
            }
        }

        private static float EnterAmount()
        {
            float num;
            do
            {
                Console.Write("Enter the amount: ");
                var input = Console.ReadLine();
                if (!float.TryParse(input, out num) || num <= 0)
                {
                    Console.WriteLine("Error: Enter a positive numeric amount!");
                    num = 0;
                }
            } while (num == 0);
            return num;
        }

    }
}
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

            Customer receiver = ScreenDisplay.DisplayCustomers();

            float amt = EnterAmount();
            
            if (!sender.CanWithdraw(amt))
            {
                Console.WriteLine("Error: Insufficient funds for transfer.");
                return;
            }

            receiver.UpdateBalance(amt, UpdateType.add);
            sender.UpdateBalance(amt, UpdateType.subtract);

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
            Customer customer = ScreenDisplay.DisplayCustomers();

            int accNum;
            do
            {
                Console.Write("Enter customer's account number: ");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out accNum) || accNum != customer.AccountNumber)
                {
                    Console.WriteLine("Error: Invalid account number!");
                    accNum = 0;
                }
            } while (accNum == 0);

            int accPin;
            do
            {
                Console.Write("Enter customer's account pin: ");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out accPin) || accPin != customer.AccountPin)
                {
                    Console.WriteLine("Error: Invalid PIN!");
                    accPin = 0;
                }
            } while (accPin == 0);

            Console.Clear();
            ScreenDisplay.WelcomeCustomerMsg(customer);
            return customer;
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
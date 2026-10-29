using System;
using System.Collections.Generic;
using Serilog;

namespace ATM
{
    public static class ScreenDisplay
    {
        public static readonly IATMactions action = new ATMactions();

        public static void WelcomeMessage()
        {
            Console.WriteLine("______________________________________________");
            Console.WriteLine("|                                             |");
            Console.WriteLine("|     Welcome! This is an ATM Application.    |");
            Console.WriteLine("|                                             |");
            Console.WriteLine("______________________________________________");
            Console.WriteLine("\n\nWelcome to your ATM App where you can manage your account!");
            PressEnterToContinue();
        }

        private static void PressEnterToContinue()
        {
            Console.WriteLine("\n\n\n\n");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        public static void WelcomeCustomerMsg(Customer customer)
        {
            Console.WriteLine($"\n\n\tCustomer: {customer.FirstName} {customer.LastName}");
            Console.WriteLine("\n\n\tWelcome to your ATM Application.");
            PressEnterToContinue();
        }

        public static void DisplayMenu(Customer customer)
        {
            Console.WriteLine("__________________________________________________________________________________");
            Console.WriteLine("|                                                                                |");
            Console.WriteLine($"| {customer.FirstName} {customer.LastName}, here are the available options:     |");
            Console.WriteLine("|                                                                                |");
            Console.WriteLine("|                                                                                |");
            Console.WriteLine("| 1. \tShow Balance                                                               |");
            Console.WriteLine("| 2. \tDeposit                                                                    |");
            Console.WriteLine("| 3. \tWithdraw                                                                   |");
            Console.WriteLine("| 4. \tTransfer                                                                   |");
            Console.WriteLine("| 5. \tShow Transaction History                                                   |");
            Console.WriteLine("| 6. \tExit                                                                       |");
            Console.WriteLine("__________________________________________________________________________________");
        }

        public static decimal EnterAmount()
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

        public static void OnboardingMessage()
        {
            Console.WriteLine("\n\n\nWelcome to your ATM App where you can manage your account.");
            Console.WriteLine("\n\nPlease follow the instructions to proceed.");
        }

        public static void Menu(Customer customer)
        {
            bool looping = true;

            while (looping)
            {
                DisplayMenu(customer);
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

                Log.Information(
                    "The user with account {AccountNumber} selected option {Option}",
                    customer.AccountNumber,
                    option);

                switch (option)
                {
                    case 1:
                        action.ShowBalance(customer);
                        break;
                    case 2:
                        action.Deposit(customer);
                        break;
                    case 3:
                        action.Withdraw(customer);
                        break;
                    case 4:
                        action.Transfer(customer);
                        break;
                    case 5:
                        action.ShowTransactionsHistory();
                        break;
                    default:
                        Log.Information("User exited menu for account {AccountNumber}", customer.AccountNumber);
                        looping = false;
                        break;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace ATM
{
    public static class ScreenDisplay
    {
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
            Console.WriteLine($"\n\n{customer.FirstName} {customer.LastName}, here are the available options: \n");
            Console.WriteLine("1. \tShow Balance");
            Console.WriteLine("2. \tDeposit");
            Console.WriteLine("3. \tWithdraw");
            Console.WriteLine("4. \tTransfer");
            Console.WriteLine("5. \tShow Transaction History");
            Console.WriteLine("6. \tExit");
        }

        public static void OnboardingMessage()
        {
            Console.WriteLine("\n\n\nWelcome to your ATM App where you can manage your account.");
            Console.WriteLine("\n\nPlease follow the instructions to proceed.");
        }
    }
}
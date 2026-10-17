using System;
using System.Collections.Generic;

namespace ATM
{
    public static class ScreenDisplay
    {
        public static void WelcomeMessage()
        {
            Console.WriteLine("Welcome! This is an ATM Application.");
            Console.WriteLine("\n\n\nWelcome to your ATM App where you can manage your account.");
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

        public static void DisplayMenu()
        {
            Console.WriteLine("\n\nHere are the available options: \n");
            Console.WriteLine("1. \tShow Balance");
            Console.WriteLine("2. \tDeposit");
            Console.WriteLine("3. \tWithdraw");
            Console.WriteLine("4. \tTransfer");
            Console.WriteLine("5. \tShow Transaction History");
            Console.WriteLine("6. \tExit");
        }

        public static Customer DisplayCustomers()
        {
            var customers = new List<Customer>
            {
                new Customer(1, "John", "Doe", 123456, 1234, 1000),
                new Customer(2, "Jane", "Smith", 654321, 4321, 2000),
                new Customer(3, "Alice", "Johnson", 111111, 1111, 3000)
            };

            Console.WriteLine("Customers that can access this ATM are:");
            foreach (var cus in customers)
            {
                Console.WriteLine($"\tCustomer Id: {cus.ID}, Name: {cus.FirstName} {cus.LastName}");
            }

            int cusId;
            do
            {
                Console.Write("Enter the Customer ID: ");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out cusId) || cusId < 1 || cusId > customers.Count)
                {
                    Console.WriteLine("Error: Invalid customer ID! Please try again: ");
                    cusId = 0;
                }
            } while (cusId == 0);

            var customer = customers[cusId - 1];
            Console.WriteLine("Account chosen: " + customer.FirstName + " " + customer.LastName);
            return customer;
        }
    }
}
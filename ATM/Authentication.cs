using System;
using System.Collections.Generic;
using System.Linq;

namespace ATM
{
    public class Authentication
    {

        public static IReadOnlyList<Customer> GetCustomers() => customers.AsReadOnly();
        private static readonly List<Customer> customers = new()
        {
            new Customer(1, "Samuel", "Obimba", 123456, 1234, 1000.00),
            new Customer(2, "Jane", "Smith", 123457, 4321, 2000.00),
            new Customer(3, "Alice", "Johnson", 123457, 5678, 1500.00),
            new Customer(4, "Bob", "Brown", 123458, 8765, 500.00)
        };

        public static Customer ExistingUser(int accountNumber, int pin)
        {
            ScreenDisplay.OnboardingMessage();
            foreach (var customer in customers)
            {
                if (customer.AccountNumber == accountNumber && customer.AccountPin == pin)
                {
                    return customer;
                }
            }
            return null;

        }

        private static readonly Random rng = new Random();
        private static readonly object rngLock = new object();

        private static int Generate6DigitAccountNumber()
        {
            int accountNumber;
            do
            {
                lock (rngLock)
                {
                    accountNumber = rng.Next(123459, 1000000);
                }
            } while (customers.Any(c => c.AccountNumber == accountNumber));
            return accountNumber;
        }

        public static Customer NewUser(string firstName, string lastName, int pin, double balance)
        {
            int accountNumber = Generate6DigitAccountNumber();
            int newId = customers.Count + 1;
            Customer newCustomer = new Customer(newId, firstName, lastName, accountNumber, pin, balance);
            customers.Add(newCustomer);
            return newCustomer;
        }

        
        public static Customer FindByAccountNumber(int accountNumber)
        {
            foreach (var customer in customers)
            {
                if (customer.AccountNumber == accountNumber)
                    return customer;
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ATM
{
    public class Customer
    {
        public int ID {  get; private set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public int AccountNumber { get; private set; }
        public double AccountBalance { get; set; }
        private float minLimitCredit = 100.00f;

        public bool CanWithdraw(float amt)
        {
            return (AccountBalance - amt) >= minLimitCredit;
        }

        public void UpdateBalance(float amt, UpdateType update)
        {
            if(update == UpdateType.add)
            {
                AccountBalance += amt;
            }
            else if (update == UpdateType.subtract)
            {
                if (AccountBalance - amt < minLimitCredit)
                {
                    throw new Exception("Insufficient funds. Minimum balance limit reached.");
                }
                else
                {
                    AccountBalance -= amt;
                }
            }
        }
        public Customer(int id, string firstName, string lastName, int accountNumber, double balance)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            AccountNumber = accountNumber;
            AccountBalance = balance;
        }
    }
    public enum UpdateType
    {
        add,
        subtract
    }
}

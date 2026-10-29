using Serilog;
using System;
using System.Linq;

namespace ATM
{
    public class ScreenDisplay
    {
        private readonly IATMactions _action;
        private readonly Authentication _auth;

        // Injected dependencies: UI depends on business logic and auth services
        public ScreenDisplay(IATMactions action, Authentication auth)
        {
            _action = action;
            _auth = auth;
        }

        public void WelcomeMessage()
        {
            Console.WriteLine("______________________________________________");
            Console.WriteLine("|                                              |");
            Console.WriteLine("|      Welcome! This is an ATM Application.    |");
            Console.WriteLine("|                                              |");
            Console.WriteLine("______________________________________________");
            Console.WriteLine("\n\nWelcome to your ATM App where you can manage your account!");
            PressEnterToContinue();
        }

        private void PressEnterToContinue()
        {
            Console.WriteLine("\n\n\n\nPress Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        public void WelcomeCustomerMsg(Customer customer)
        {
            Console.WriteLine($"\n\n\tCustomer: {customer.FirstName} {customer.LastName}");
            Console.WriteLine("\n\n\tWelcome to your ATM Application.");
            PressEnterToContinue();
        }

        public void OnboardingMessage()
        {
            Console.WriteLine("\n\n\nWelcome to your ATM App where you can manage your account.");
            Console.WriteLine("\n\nPlease follow the instructions to proceed.");
        }

        public decimal EnterAmount()
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

        public Customer? Login()
        {
            while (true)
            {
                OnboardingMessage();

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

                    var customer = _auth.ExistingUser(accNum, accPin);
                    if (customer != null)
                    {
                        Console.Clear();
                        WelcomeCustomerMsg(customer);
                        return customer;
                    }

                    Console.WriteLine("No matching customer found. Try again or create a new user.");
                    continue;
                }
                else
                {
                    string firstName;
                    do
                    {
                        Console.Write("Please enter your first name: ");
                        firstName = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(firstName) || !firstName.All(char.IsLetter))
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
                        if (string.IsNullOrWhiteSpace(lastName) || !lastName.All(char.IsLetter))
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
                            Console.WriteLine("Error: Invalid PIN! Enter a four digit pin");
                            pin = "";
                        }
                    } while (string.IsNullOrEmpty(pin));

                    decimal initialBalance = 0;
                    var newCustomer = _auth.NewUser(firstName, lastName, pin, initialBalance);
                    Console.Clear();
                    Console.WriteLine($"\n\nYour account has been created successfully! Your account number is: {newCustomer.AccountNumber}");
                    WelcomeCustomerMsg(newCustomer);
                    return newCustomer;
                }
            }
        }

        public void DisplayMenu(Customer customer)
        {
            Console.WriteLine("__________________________________________________________________________________");
            Console.WriteLine("|                                                                                |");
            Console.WriteLine($"| {customer.FirstName} {customer.LastName}, here are the available options:     |");
            Console.WriteLine("|                                                                                |");
            Console.WriteLine("| 1. \tShow Balance                                                             |");
            Console.WriteLine("| 2. \tDeposit                                                                  |");
            Console.WriteLine("| 3. \tWithdraw                                                                 |");
            Console.WriteLine("| 4. \tTransfer                                                                 |");
            Console.WriteLine("| 5. \tShow Transaction History                                                 |");
            Console.WriteLine("| 6. \tExit                                                                     |");
            Console.WriteLine("__________________________________________________________________________________");
        }

        public void Menu(Customer customer)
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
                        _action.ShowBalance(customer);
                        break;

                    case 2:
                        Console.WriteLine("\nThis is the section for depositing money.");
                        decimal depositAmount = EnterAmount();
                        _action.Deposit(customer, depositAmount);
                        break;

                    case 3:
                        Console.WriteLine("\nThis is the section for withdrawing money.");
                        decimal withdrawAmount = EnterAmount();
                        _action.Withdraw(customer, withdrawAmount);
                        break;

                    case 4:
                        Console.WriteLine("\nThis is the section for transferring money.");
                        int receiverAccount;
                        do
                        {
                            Console.Write("Enter receiver account number: ");
                            var accInput = Console.ReadLine();
                            if (!int.TryParse(accInput, out receiverAccount))
                            {
                                Console.WriteLine("Error: Invalid account number!");
                                receiverAccount = 0;
                            }
                        } while (receiverAccount == 0);

                        decimal transferAmount = EnterAmount();
                        _action.Transfer(customer, receiverAccount, transferAmount);
                        break;

                    case 5:
                        _action.ShowTransactionsHistory();
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
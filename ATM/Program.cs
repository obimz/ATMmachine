using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;


namespace ATM
{
    public class Program
    {
        public static void Main(string[] args)
        {


            ScreenDisplay.WelcomeMessage();

            Customer customer = ATMactions.Login();

            ATMactions.Menu(customer);


        }
    }
}
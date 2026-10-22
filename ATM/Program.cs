using Microsoft.Data.SqlClient;


namespace ATM
{
    public class Program
    {
        public static void Main(string[] args)
        {


            ScreenDisplay.WelcomeMessage();

            Customer customer = ATMactions.Login();

            ATMactions.Menu(customer);

            /*try
            {                
                using (SqlConnection conn = DBconnect.GetConnection())
                {
                    Console.WriteLine("Connection successful! Ready to query the database");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Failed to connect to the database: {ex.Message}");
            }*/


        }
    }
}
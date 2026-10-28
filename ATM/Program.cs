using Serilog;
using Serilog.Events;



namespace ATM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string logDirectory = @"C:\Users\Obimz01\source\repos\ATM\logs";
            Directory.CreateDirectory(logDirectory);

            string logFilePath = Path.Combine(logDirectory, "atm-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14)
                .CreateLogger();



            try
            {
                Log.Information("Atm Application has started");
                ScreenDisplay.WelcomeMessage();

                Customer customer = ATMactions.Login();

                ScreenDisplay.Menu(customer);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "ATM application ended unexpectedly");
            }
            finally
            {
                Log.Information("ATM application shutting down...");
                Log.CloseAndFlush();
            }

        }
    }
}
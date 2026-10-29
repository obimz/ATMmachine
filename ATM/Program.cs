using System;
using System.IO;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;

namespace ATM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Logging setup
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
                Log.Information("ATM Application has started");

                // Dependency Injection Setup
                var services = new ServiceCollection();

                services.AddTransient<IAccountReadRepository, DBQueries>();
                services.AddTransient<ITransactionsCommandsRepo, DBQueries>();

                services.AddTransient<Authentication>();
                services.AddTransient<ScreenDisplay>();
                services.AddTransient<IATMactions, ATMactions>();

                ServiceProvider serviceProvider = services.BuildServiceProvider();

                // Resolves services from the DI container
                var screenDisplay = serviceProvider.GetRequiredService<ScreenDisplay>();

                screenDisplay.WelcomeMessage();

                Customer? customer = screenDisplay.Login();

                if (customer is not null)
                {
                    screenDisplay.Menu(customer);
                }
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
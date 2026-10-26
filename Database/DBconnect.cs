using Microsoft.Data.SqlClient;
using Serilog;

namespace ATM
{
    public static class DBconnect
    {
        private const string ConnectionString =
            "Server=localhost\\SQLEXPRESS01;Initial Catalog=ATMmachine;Integrated Security=True;Trust Server Certificate=True";

        public static SqlConnection GetConnection()
        {
            try
            {
                Log.Information("Opening SQL connection to ATM database");

                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();

                Log.Information("SQL connection opened successfully");
                return connection;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open SQL connection");
                throw;
            }
        }
    }
}
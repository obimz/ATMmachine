using Microsoft.Data.SqlClient;

namespace ATM
{
    public static class DBconnect
    {
        private const string ConnectionString =
            "Server=localhost\\SQLEXPRESS01;Initial Catalog=ATMmachine;Integrated Security=True;Trust Server Certificate=True";

        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
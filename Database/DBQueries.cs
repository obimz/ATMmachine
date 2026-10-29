using Microsoft.Data.SqlClient;
using System.Data;

namespace ATM
{
	public sealed record AccountData(
		int CustomerId,
		int AccountId,
		string FirstName,
		string LastName,
		int AccountNumber,
		byte[] PinHash,
		decimal Balance,
		decimal MinimumBalance,
		bool IsActive);

	public sealed record TransactionData(
		string TransactionType,
		decimal? Amount,
		decimal BalanceBefore,
		decimal BalanceAfter,
		DateTime TransactionDate,
		string? Description);

	public class DBQueries : IAccountReadRepository, ITransactionsCommandsRepo
	{
		public AccountData? FindAccount(int accountNumber)
		{
			const string query = """
				SELECT
					c.CustomerId,
					a.AccountId,
					c.FirstName,
					c.LastName,
					a.AccountNumber,
					a.PinHash,
					a.Balance,
					a.MinimumBalance,
					a.IsActive
				FROM Accounts AS a
				INNER JOIN Customers AS c ON c.CustomerId = a.CustomerId
				WHERE a.AccountNumber = @AccountNumber;
				""";

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlCommand command = new SqlCommand(query, connection);
			command.Parameters.Add("@AccountNumber", SqlDbType.Int).Value = accountNumber;

			using SqlDataReader reader = command.ExecuteReader();
			return reader.Read() ? ReadAccount(reader) : null;
		}

		public AccountData CreateAccount(
			string firstName,
			string lastName,
			int accountNumber,
			byte[] pinHash,
			decimal initialBalance = 0)
		{
			const string customerQuery = """
				INSERT INTO Customers (FirstName, LastName)
				OUTPUT INSERTED.CustomerId
				VALUES (@FirstName, @LastName);
				""";

			const string accountQuery = """
				INSERT INTO Accounts
					(CustomerId, AccountNumber, PinHash, Balance)
				OUTPUT INSERTED.AccountId
				VALUES
					(@CustomerId, @AccountNumber, @PinHash, @Balance);
				""";

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlTransaction transaction = connection.BeginTransaction();

			try
			{
				int customerId;

				using (SqlCommand customerCommand = new SqlCommand(
					customerQuery,
					connection,
					transaction))
				{
					customerCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = firstName;
					customerCommand.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = lastName;
					customerId = Convert.ToInt32(customerCommand.ExecuteScalar());
				}

				int accountId;

				using (SqlCommand accountCommand = new SqlCommand(
					accountQuery,
					connection,
					transaction))
				{
					accountCommand.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
					accountCommand.Parameters.Add("@AccountNumber", SqlDbType.Int).Value = accountNumber;
					accountCommand.Parameters.Add("@PinHash", SqlDbType.VarBinary, 256).Value = pinHash;

					SqlParameter balanceParameter = accountCommand.Parameters.Add(
						"@Balance",
						SqlDbType.Decimal);
					balanceParameter.Precision = 19;
					balanceParameter.Scale = 4;
					balanceParameter.Value = initialBalance;

					accountId = Convert.ToInt32(accountCommand.ExecuteScalar());
				}

				transaction.Commit();

				return FindAccountById(accountId)
					?? throw new InvalidOperationException("The new account could not be loaded.");
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public AccountData ApplyDeposit(int accountId, decimal amount)
		{
			return ApplyBalanceChange(
				accountId,
				amount,
				"Deposit",
				amount,
				"Cash deposit");
		}

		public AccountData ApplyWithdrawal(int accountId, decimal amount)
		{
			return ApplyBalanceChange(
				accountId,
				-amount,
				"Withdrawal",
				amount,
				"ATM withdrawal");
		}

		public void Transfer(int senderAccountId, int receiverAccountId, decimal amount)
		{
			const string balanceQuery = """
				SELECT AccountId, Balance, MinimumBalance
				FROM Accounts WITH (UPDLOCK, ROWLOCK)
				WHERE AccountId IN (@SenderAccountId, @ReceiverAccountId)
				ORDER BY AccountId;
				""";

			const string updateQuery = """
				UPDATE Accounts
				SET Balance = Balance + @Delta
				WHERE AccountId = @AccountId;
				""";

			const string transactionQuery = """
				INSERT INTO Transactions
				(
					TransactionType,
					AccountId,
					RelatedAccountId,
					Amount,
					BalanceBefore,
					BalanceAfter,
					Description
				)
				VALUES
				(
					'Transfer',
					@AccountId,
					@RelatedAccountId,
					@Amount,
					@BalanceBefore,
					@BalanceAfter,
					@Description
				);
				""";

			if (senderAccountId == receiverAccountId)
			{
				throw new InvalidOperationException("The sender and receiver must be different accounts.");
			}

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlTransaction transaction = connection.BeginTransaction();

			try
			{
				Dictionary<int, (decimal Balance, decimal MinimumBalance)> accounts = new();

				using (SqlCommand balanceCommand = new SqlCommand(
					balanceQuery,
					connection,
					transaction))
				{
					balanceCommand.Parameters.Add("@SenderAccountId", SqlDbType.Int).Value = senderAccountId;
					balanceCommand.Parameters.Add("@ReceiverAccountId", SqlDbType.Int).Value = receiverAccountId;

					using SqlDataReader reader = balanceCommand.ExecuteReader();
					while (reader.Read())
					{
						accounts[reader.GetInt32(0)] = (
							reader.GetDecimal(1),
							reader.GetDecimal(2));
					}
				}

				if (!accounts.TryGetValue(senderAccountId, out var sender)
					|| !accounts.TryGetValue(receiverAccountId, out var receiver))
				{
					throw new InvalidOperationException("Both transfer accounts must exist.");
				}

				if (amount <= 0)
				{
					throw new ArgumentException("Transfer amount must be positive.", nameof(amount));
				}

				decimal senderBalanceAfter = sender.Balance - amount;
				if (senderBalanceAfter < sender.MinimumBalance)
				{
					throw new InvalidOperationException("Insufficient funds for transfer.");
				}

				decimal receiverBalanceAfter = receiver.Balance + amount;

				UpdateBalance(connection, transaction, updateQuery, senderAccountId, -amount);
				UpdateBalance(connection, transaction, updateQuery, receiverAccountId, amount);

				InsertTransaction(
					connection,
					transaction,
					transactionQuery,
					senderAccountId,
					receiverAccountId,
					amount,
					sender.Balance,
					senderBalanceAfter,
					"Transfer sent");

				InsertTransaction(
					connection,
					transaction,
					transactionQuery,
					receiverAccountId,
					senderAccountId,
					amount,
					receiver.Balance,
					receiverBalanceAfter,
					"Transfer received");

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public List<TransactionData> GetTransactions(int accountId)
		{
			const string query = """
				SELECT
					TransactionType,
					Amount,
					BalanceBefore,
					BalanceAfter,
					TransactionDate,
					Description
				FROM Transactions
				WHERE AccountId = @AccountId
				ORDER BY TransactionDate DESC;
				""";

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlCommand command = new SqlCommand(query, connection);
			command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

			using SqlDataReader reader = command.ExecuteReader();
			List<TransactionData> transactions = new();

			while (reader.Read())
			{
				transactions.Add(new TransactionData(
					reader.GetString(0),
					reader.IsDBNull(1) ? null : reader.GetDecimal(1),
					reader.GetDecimal(2),
					reader.GetDecimal(3),
					reader.GetDateTime(4),
					reader.IsDBNull(5) ? null : reader.GetString(5)));
			}

			return transactions;
		}

		private static AccountData ApplyBalanceChange(
			int accountId,
			decimal delta,
			string transactionType,
			decimal amount,
			string description)
		{
			const string accountQuery = """
				SELECT Balance, MinimumBalance
				FROM Accounts WITH (UPDLOCK, ROWLOCK)
				WHERE AccountId = @AccountId;
				""";

			const string updateQuery = """
				UPDATE Accounts
				SET Balance = @BalanceAfter
				WHERE AccountId = @AccountId;
				""";

			const string transactionQuery = """
				INSERT INTO Transactions
				(
					TransactionType,
					AccountId,
					Amount,
					BalanceBefore,
					BalanceAfter,
					Description
				)
				VALUES
				(
					@TransactionType,
					@AccountId,
					@Amount,
					@BalanceBefore,
					@BalanceAfter,
					@Description
				);
				""";

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlTransaction transaction = connection.BeginTransaction();

			try
			{
				decimal balanceBefore;
				decimal minimumBalance;

				using (SqlCommand accountCommand = new SqlCommand(
					accountQuery,
					connection,
					transaction))
				{
					accountCommand.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

					using SqlDataReader reader = accountCommand.ExecuteReader();
					if (!reader.Read())
					{
						throw new InvalidOperationException("The account does not exist.");
					}

					balanceBefore = reader.GetDecimal(0);
					minimumBalance = reader.GetDecimal(1);
				}

				decimal balanceAfter = balanceBefore + delta;
				if (balanceAfter < minimumBalance)
				{
					throw new InvalidOperationException("Insufficient funds. Minimum balance limit reached.");
				}

				using (SqlCommand updateCommand = new SqlCommand(
					updateQuery,
					connection,
					transaction))
				{
					updateCommand.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
					AddDecimalParameter(updateCommand, "@BalanceAfter", balanceAfter);
					updateCommand.ExecuteNonQuery();
				}

				using (SqlCommand transactionCommand = new SqlCommand(
					transactionQuery,
					connection,
					transaction))
				{
					transactionCommand.Parameters.Add("@TransactionType", SqlDbType.VarChar, 20).Value = transactionType;
					transactionCommand.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
					AddDecimalParameter(transactionCommand, "@Amount", amount);
					AddDecimalParameter(transactionCommand, "@BalanceBefore", balanceBefore);
					AddDecimalParameter(transactionCommand, "@BalanceAfter", balanceAfter);
					transactionCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = description;
					transactionCommand.ExecuteNonQuery();
				}

				transaction.Commit();
				return FindAccountById(accountId)
					?? throw new InvalidOperationException("The updated account could not be loaded.");
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		private static AccountData? FindAccountById(int accountId)
		{
			const string query = """
				SELECT
					c.CustomerId,
					a.AccountId,
					c.FirstName,
					c.LastName,
					a.AccountNumber,
					a.PinHash,
					a.Balance,
					a.MinimumBalance,
					a.IsActive
				FROM Accounts AS a
				INNER JOIN Customers AS c ON c.CustomerId = a.CustomerId
				WHERE a.AccountId = @AccountId;
				""";

			using SqlConnection connection = DBconnect.GetConnection();
			using SqlCommand command = new SqlCommand(query, connection);
			command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

			using SqlDataReader reader = command.ExecuteReader();
			return reader.Read() ? ReadAccount(reader) : null;
		}

		private static AccountData ReadAccount(SqlDataReader reader)
		{
			return new AccountData(
				reader.GetInt32(0),
				reader.GetInt32(1),
				reader.GetString(2),
				reader.GetString(3),
				reader.GetInt32(4),
				reader.GetFieldValue<byte[]>(5),
				reader.GetDecimal(6),
				reader.GetDecimal(7),
				reader.GetBoolean(8));
		}

		private static void UpdateBalance(
			SqlConnection connection,
			SqlTransaction transaction,
			string query,
			int accountId,
			decimal delta)
		{
			using SqlCommand command = new SqlCommand(query, connection, transaction);
			command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
			AddDecimalParameter(command, "@Delta", delta);
			command.ExecuteNonQuery();
		}

		private static void InsertTransaction(
			SqlConnection connection,
			SqlTransaction transaction,
			string query,
			int accountId,
			int relatedAccountId,
			decimal amount,
			decimal balanceBefore,
			decimal balanceAfter,
			string description)
		{
			using SqlCommand command = new SqlCommand(query, connection, transaction);
			command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
			command.Parameters.Add("@RelatedAccountId", SqlDbType.Int).Value = relatedAccountId;
			AddDecimalParameter(command, "@Amount", amount);
			AddDecimalParameter(command, "@BalanceBefore", balanceBefore);
			AddDecimalParameter(command, "@BalanceAfter", balanceAfter);
			command.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = description;
			command.ExecuteNonQuery();
		}

		private static void AddDecimalParameter(
			SqlCommand command,
			string name,
			decimal value)
		{
			SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
			parameter.Precision = 19;
			parameter.Scale = 4;
			parameter.Value = value;
		}
	}
}

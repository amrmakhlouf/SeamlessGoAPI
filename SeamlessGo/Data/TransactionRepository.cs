using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;
        private readonly ITransactionLineRepository _TransactionLineRepository;

        public TransactionRepository(IConfiguration configuration, ITransactionLineRepository TransactionLineRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _TransactionLineRepository = TransactionLineRepository;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(DateTime? LastModifiedUtc)
        {
            var Transactions = new List<Transaction>();

            using var connection = new SqlConnection(_connectionString);

            var query = "SELECT * FROM dbo.Transactions WHERE 1=1";
            if (LastModifiedUtc.HasValue)
            {
                query += " AND LastModifiedUtc > @LastModifiedUtc";
            }
            query += " ORDER BY TransactionID DESC";
            using var command = new SqlCommand(query, connection);

            //using var command = new SqlCommand("SELECT * FROM dbo.Transactions  where 1=1 and  Transaction BY TransactionID DESC", connection);
            if (LastModifiedUtc.HasValue)
            {
                command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = LastModifiedUtc.Value;
            }
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Transactions.Add(MapReaderToTransaction(reader));
            }

            return Transactions;
        }

        public async Task<Transaction?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Transactions WHERE TransactionID = @TransactionID", connection);

            command.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToTransaction(reader);
            }

            return null;
        }

        public async Task<Transaction?> GetByIdWithLinesAsync(string id)
        {
            var Transaction = await GetByIdAsync(id);

            if (Transaction != null)
            {
                Transaction.TransactionLine = (await _TransactionLineRepository.GetByTransactionIdAsync(id)).ToList();
            }

            return Transaction;
        }

        public async Task<Transaction> CreateAsync(Transaction Transaction, List<TransactionLine>? TransactionLines)
        {
            const string sql = @"
                INSERT INTO dbo.Transactions 
                (TransactionID,CustomerID, TransactionDate, UpdatedDate, TransactionTypeID, SubTotal, TotalAmount, GrossAmount, 
                 TotalRemainingAmount, DiscountAmount, DiscountPerc, NetAmount, Tax, TaxPerc, 
                 Status, CreatedByUserID, RouteID, IsVoided, Note, SourceTransactionID,SyncStatus,LastModifiedUtc)
                VALUES 
                (@TransactionID,@CustomerID, @TransactionDate, @UpdateDate, @TransactionTypeID,@SubTotal, @TotalAmount, @GrossAmount,
                 @TotalRemainingAmount, @DiscountAmount, @DiscountPerc, @NetAmount, @Tax, @TaxPerc,
                 @Status, @CreatedByUserID, @RouteID, @IsVoided, @Note, null,@SyncStatus,@LastModifiedUtc);
                ";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert Transaction with user-provided TransactionID
                using var command = new SqlCommand(sql, connection, transaction);

                AddTransactionParameters(command, Transaction);

                // Execute INSERT - no need to get SCOPE_IDENTITY
                await command.ExecuteNonQueryAsync();

                // Use the TransactionID that user provided (already in Transaction.TransactionID)
                string newTransactionId = Transaction.TransactionID;

                // Insert Transaction lines if provided
                if (TransactionLines != null && TransactionLines.Any())
                {
                    foreach (var line in TransactionLines)
                    {
                        line.TransactionID = newTransactionId;  // Assign the user-provided TransactionID
                        await CreateTransactionLineAsync(connection, transaction, line);
                    }
                }
                transaction.Commit();
                return Transaction;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        //public async Task<IEnumerable<Transactions>> GetTransactionsByCustomerAsync(string customerId)
        //{
        //    var Transactions = new List<Transactions>();

        //    const string sql = "SELECT * FROM dbo.Transactions WHERE CustomerID = @CustomerID Transaction BY TransactionDate DESC";

        //    using var connection = new SqlConnection(_connectionString);
        //    using var command = new SqlCommand(sql, connection);

        //    command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = customerId;

        //    await connection.OpenAsync();
        //    using var reader = await command.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        Transactions.Add(MapReaderToTransaction(reader));
        //    }

        //    return Transactions;
        //}

        // Private helper method - not part of interface
        // Used internally by CreateAsync to insert Transaction lines
        private async Task CreateTransactionLineAsync(SqlConnection connection, SqlTransaction transaction, TransactionLine line)
        {

            const string sql = @"
                INSERT INTO dbo.TransactionLines 
                (TransactionLineID,TransactionID, ItemPackID, DiscountAmount, DiscountPerc, Bonus, Quantity, ItemPrice, FullPrice, TotalPrice, Note)
                VALUES 
                (@TransactionLineID,@TransactionID, @ItemPackID, @DiscountAmount, @DiscountPerc, @Bonus, @Quantity, @ItemPrice, @FullPrice, 0, @Note);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@TransactionLineID", SqlDbType.NVarChar).Value = line.TransactionLineID;

            command.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = line.TransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPackID", SqlDbType.NVarChar).Value = line.ItemPackID ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = line.DiscountAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountPerc", SqlDbType.Float).Value = line.DiscountPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@Bonus", SqlDbType.Int).Value = line.Bonus ?? (object)DBNull.Value;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = line.Quantity ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPrice", SqlDbType.Decimal).Value = line.ItemPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@FullPrice", SqlDbType.Decimal).Value = line.FullPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalPrice", SqlDbType.Decimal).Value = line.TotalPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = line.Note ?? (object)DBNull.Value;

            await command.ExecuteScalarAsync();



        }

        private static Transaction MapReaderToTransaction(SqlDataReader reader)
        {
            return new Transaction
            {

                TransactionID = reader.IsDBNull("TransactionID") ? null : reader.GetString("TransactionID"),
                CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetGuid("CustomerID").ToString(),
                TransactionDate = reader.IsDBNull("TransactionDate") ? null : reader.GetDateTime("TransactionDate"),
                UpdateDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                TransactionTypeID = reader.IsDBNull("TransactionTypeID") ? null : reader.GetInt32("TransactionTypeID"),
                SubTotal = reader.IsDBNull("SubTotal") ? null : reader.GetDecimal("SubTotal"),
                TotalAmount = reader.IsDBNull("TotalAmount") ? null : reader.GetDecimal("TotalAmount"),
                GrossAmount = reader.IsDBNull("GrossAmount") ? null : reader.GetDecimal("GrossAmount"),
                TotalRemainingAmount = reader.IsDBNull("TotalRemainingAmount") ? null : reader.GetDecimal("TotalRemainingAmount"),
                DiscountAmount = reader.IsDBNull("DiscountAmount") ? null : reader.GetDecimal("DiscountAmount"),
                DiscountPerc = reader.IsDBNull("DiscountPerc") ? null : (float)reader.GetDecimal("DiscountPerc"),
                NetAmount = reader.IsDBNull("NetAmount") ? null : reader.GetDecimal("NetAmount"),
                Tax = reader.IsDBNull("Tax") ? null : reader.GetDecimal("Tax"),
                TaxPerc = reader.IsDBNull("TaxPerc") ? null : (float)reader.GetDecimal("TaxPerc"),

                // ✅ tinyint -> byte -> int
                Status = reader.IsDBNull("Status") ? null : (int?)reader.GetByte("Status"),

                CreatedByUserID = reader.IsDBNull("CreatedByUserID") ? null : reader.GetInt32("CreatedByUserID"),
                RouteID = reader.IsDBNull("RouteID") ? null : reader.GetInt32("RouteID"),
                IsVoided = reader.IsDBNull("IsVoided") ? null : reader.GetBoolean("IsVoided"),
                Note = reader.IsDBNull("Note") ? null : reader.GetString("Note"),
                SourceTransactionID = reader.IsDBNull("SourceTransactionID") ? null : reader.GetString("SourceTransactionID"),
                SourceOrderID = reader.IsDBNull("SourceOrderID") ? null : reader.GetString("SourceOrderID"),
                LastModifiedUtc = reader.IsDBNull("LastModifiedUtc") ? DateTime.MinValue : reader.GetDateTime("LastModifiedUtc"),

                // ✅ tinyint -> byte -> int
            };
        }

        private static void AddTransactionParameters(SqlCommand command, Transaction Transaction)

        {
            command.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = Transaction.TransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = Transaction.CustomerID ?? (object)DBNull.Value;
            command.Parameters.Add("@TransactionDate", SqlDbType.DateTime2).Value = Transaction.TransactionDate ?? (object)DBNull.Value;
            command.Parameters.Add("@UpdateDate", SqlDbType.DateTime2).Value = DateTime.Now;
            command.Parameters.Add("@TransactionTypeID", SqlDbType.Int).Value = Transaction.TransactionTypeID ?? (object)DBNull.Value;
            command.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = Transaction.SubTotal ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = Transaction.TotalAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@GrossAmount", SqlDbType.Decimal).Value = Transaction.GrossAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalRemainingAmount", SqlDbType.Decimal).Value = Transaction.TotalRemainingAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = Transaction.DiscountAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountPerc", SqlDbType.Float).Value = Transaction.DiscountPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = Transaction.NetAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@Tax", SqlDbType.Decimal).Value = Transaction.Tax ?? (object)DBNull.Value;
            command.Parameters.Add("@TaxPerc", SqlDbType.Float).Value = Transaction.TaxPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@Status", SqlDbType.TinyInt).Value = Transaction.Status ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedByUserID", SqlDbType.NVarChar).Value = Transaction.CreatedByUserID ?? (object)DBNull.Value;
            command.Parameters.Add("@RouteID", SqlDbType.Int).Value = Transaction.RouteID ?? (object)DBNull.Value;
            command.Parameters.Add("@IsVoided", SqlDbType.Bit).Value = Transaction.IsVoided ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = Transaction.Note ?? (object)DBNull.Value;
            command.Parameters.Add("@SourceTransactionID", SqlDbType.NVarChar).Value = Transaction.SourceTransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@SourceOrderID", SqlDbType.NVarChar).Value = Transaction.SourceOrderID ?? (object)DBNull.Value;
            command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = Transaction.LastModifiedUtc ?? (object)DBNull.Value;

        }
    }
}



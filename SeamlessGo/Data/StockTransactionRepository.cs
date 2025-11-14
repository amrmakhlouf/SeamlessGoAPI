using SeamlessGo.Models;
using System.Data.SqlClient;
using System.Data;

namespace SeamlessGo.Data
{
    public class StockTransactionRepository: IStockTransactionRepository
    {

       
            private readonly string _connectionString;
            private readonly IStockTransactionLineRepository _StockTransactionLineRepository;

            public StockTransactionRepository(IConfiguration configuration, IStockTransactionLineRepository StockTransactionLineRepository)
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentException("Connection string not found");
                _StockTransactionLineRepository = StockTransactionLineRepository;
            }

            public async Task<IEnumerable<StockTransaction>> GetAllAsync( DateTime? LastModifiedUtc)
            {
                var StockTransactions = new List<StockTransaction>();

            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM dbo.StockTransactions where 1=1";
            if (LastModifiedUtc.HasValue) { query += " and LastModifiedUtc >@LastModifiedUtc"; }
            query += " ORDER BY Transactiondate asc";
            using var command = new SqlCommand(query, connection);
            if (LastModifiedUtc.HasValue)
            {
                command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = LastModifiedUtc;
            }
            await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    StockTransactions.Add(MapReaderToStockTransaction(reader));
                }

                return StockTransactions;
            }

            public async Task<StockTransaction?> GetByIdAsync(string id)
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("SELECT * FROM dbo.StockTransactions WHERE StockTransactionID = @StockTransactionID", connection);

                command.Parameters.Add("@StockTransactionID", SqlDbType.NVarChar).Value = id;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapReaderToStockTransaction(reader);
                }

                return null;
            }

            public async Task<StockTransaction?> GetByIdWithLinesAsync(string id)
            {
                var StockTransaction = await GetByIdAsync(id);

                if (StockTransaction != null)
                {
                    StockTransaction.StockTransactionLine = (await _StockTransactionLineRepository.GetByTransactionIdAsync(id)).ToList();
                }

                return StockTransaction;
            }

        public async Task<StockTransaction> CreateAsync(StockTransaction stockTransaction, List<StockTransactionLine>? stockTransactionLines)
        {
            const string sql = @"
        INSERT INTO dbo.StockTransactions 
        (StockTransactionID, SupplierID, SourceLocationID, DeliveryLocationID, Status, 
         StockTransactionTypeID, TransactionDate, UpdatedDate, TotalQuantity, TotalAmount, 
         SupplierInvoiceNumber, DeliveryDate, Note, Subtotal,GrossAmount,TotalRemainingAmount, DiscountAmount, DiscountPerc, 
         ShippingCost, ImportDuty, RouteID, CreatedByUserID, SyncStatus,LastModifiedUtc)
        VALUES 
        (@StockTransactionID, @SupplierID, @SourceLocationID, @DestinationLocationID, @Status,
         @StockTransactionTypeID, @TransactionDate, @UpdatedDate, @TotalQuantity, @TotalAmount,
         @SupplierInvoiceNumber, @DeliveryDate, @Note, @Subtotal,@GrossAmount,@TotalRemainingAmount, @DiscountAmount, @DiscountPerc,
         @ShippingCost, @ImportDuty, @RouteID, @CreatedByUserID, @SyncStatus,@LastModifiedUtc);";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();  // FIXED: Renamed from stocktransaction to transaction

            try
            {
                // Set UpdatedDate if not provided
                if (!stockTransaction.UpdateDate.HasValue)
                {
                    stockTransaction.UpdateDate = DateTime.Now;
                }

                // Insert StockTransaction with user-provided StockTransactionID
                using var command = new SqlCommand(sql, connection, transaction);
                AddStockTransactionParameters(command, stockTransaction);

                // Execute INSERT
                await command.ExecuteNonQueryAsync();

                // Use the StockTransactionID that user provided
                string newStockTransactionId = stockTransaction.StockTransactionID;

                // Insert StockTransaction lines if provided
                if (stockTransactionLines != null && stockTransactionLines.Any())
                {
                    foreach (var line in stockTransactionLines)
                    {
                        line.StockTransactionID = newStockTransactionId;
                        await CreateStockTransactionLineAsync(connection, transaction, line);
                    }
                }

                transaction.Commit();
                return stockTransaction;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        //public async Task<IEnumerable<StockTransactions>> GetStockTransactionsByCustomerAsync(string customerId)
        //{
        //    var StockTransactions = new List<StockTransactions>();

        //    const string sql = "SELECT * FROM dbo.StockTransactions WHERE CustomerID = @CustomerID StockTransaction BY StockTransactionDate DESC";

        //    using var connection = new SqlConnection(_connectionString);
        //    using var command = new SqlCommand(sql, connection);

        //    command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = customerId;

        //    await connection.OpenAsync();
        //    using var reader = await command.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        StockTransactions.Add(MapReaderToStockTransaction(reader));
        //    }

        //    return StockTransactions;
        //}

        // Private helper method - not part of interface
        // Used internally by CreateAsync to insert StockTransaction lines
        private async Task CreateStockTransactionLineAsync(SqlConnection connection, SqlTransaction StockTransaction, StockTransactionLine line)
            {

                const string sql = @"
                INSERT INTO dbo.StockTransactionLines 
                (StockTransactionLineID,StockTransactionID, ItemPackID, Quantity, Coast, TotalCost, ExpirationDate)
                VALUES 
                (@StockTransactionLineID,@StockTransactionID, @ItemPackID, @Quantity, @Coast, @TotalCost, @ExpirationDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var command = new SqlCommand(sql, connection, StockTransaction);

            command.Parameters.Add("@StockTransactionLineID", SqlDbType.NVarChar).Value = line.StockTransactionLineID;
            command.Parameters.Add("@StockTransactionID", SqlDbType.NVarChar).Value = line.StockTransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPackID", SqlDbType.NVarChar).Value = line.ItemPackID ?? (object)DBNull.Value;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = line.Quantity ?? (object)DBNull.Value;
            command.Parameters.Add("@Coast", SqlDbType.Decimal).Value = line.Coast ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalCost", SqlDbType.Decimal).Value = line.TotalCost ?? (object)DBNull.Value;
            command.Parameters.Add("@ExpirationDate", SqlDbType.DateTime2).Value = line.ExpirationDate ?? (object)DBNull.Value;

           

                await command.ExecuteScalarAsync();



            }

            private static StockTransaction MapReaderToStockTransaction(SqlDataReader reader)
            {
                return new StockTransaction
                {

                    StockTransactionID = reader.IsDBNull("StockTransactionID") ? null : reader.GetString("StockTransactionID"),
                    SupplierID = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                    SourceLocationID = reader.IsDBNull("SourceLocationID") ? null : reader.GetInt32("SourceLocationID"),
                    DeliveryLocationID = reader.IsDBNull("DeliveryLocationID") ? null : reader.GetInt32("DeliveryLocationID"),
                    Status = reader.IsDBNull("Status") ? null : (int?)reader.GetByte("Status"),
                    StockTransactionTypeID = reader.IsDBNull("StockTransactionTypeID") ? null : reader.GetInt32("StockTransactionTypeID"),
                    TransactionDate = reader.IsDBNull("TransactionDate") ? null : reader.GetDateTime("TransactionDate"),
                    UpdateDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                    TotalQuantity = reader.IsDBNull("TotalQuantity") ? null : reader.GetDecimal("TotalQuantity"),

                    TotalAmount = reader.IsDBNull("TotalAmount") ? null : reader.GetDecimal("TotalAmount"),
                    SupplierInvoiceNumber = reader.IsDBNull("SupplierInvoiceNumber") ? null : reader.GetString("SupplierInvoiceNumber"),
                    DeliveryDate = reader.IsDBNull("DeliveryDate") ? null : reader.GetDateTime("DeliveryDate"),
                    Note = reader.IsDBNull("Note") ? null : reader.GetString("Note"),

                    SubTotal = reader.IsDBNull("SubTotal") ? null : reader.GetDecimal("SubTotal"),
                    GrossAmount = reader.IsDBNull("GrossAmount") ? null : reader.GetDecimal("GrossAmount"),
                    TotalRemainingAmount = reader.IsDBNull("TotalRemainingAmount") ? null : reader.GetDecimal("TotalRemainingAmount"),
                    DiscountAmount = reader.IsDBNull("DiscountAmount") ? null : reader.GetDecimal("DiscountAmount"),
                    DiscountPerc = reader.IsDBNull("DiscountPerc") ? null : (float)reader.GetDecimal("DiscountPerc"),
                    ShippingCost = reader.IsDBNull("ShippingCost") ? null : reader.GetDecimal("ShippingCost"),
                    ImportDuty = reader.IsDBNull("ImportDuty") ? null : reader.GetDecimal("ImportDuty"),

                    RouteID = reader.IsDBNull("RouteID") ? null : reader.GetInt32("RouteID"),
                    CreatedByUserID = reader.IsDBNull("CreatedByUserID") ? null : reader.GetInt32("CreatedByUserID"),
                    LastModifiedUtc = reader.IsDBNull("LastModifiedUtc") ? DateTime.MinValue : reader.GetDateTime("LastModifiedUtc"),



                };
            }

        private static void AddStockTransactionParameters(SqlCommand command, StockTransaction stockTransaction)
        {
            command.Parameters.Add("@StockTransactionID", SqlDbType.NVarChar, 50).Value = stockTransaction.StockTransactionID;  // Required
            command.Parameters.Add("@SupplierID", SqlDbType.Int).Value = stockTransaction.SupplierID ?? (object)DBNull.Value;  // FIXED: Was CustomerID
            command.Parameters.Add("@SourceLocationID", SqlDbType.Int).Value = stockTransaction.SourceLocationID ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@DeliveryLocationID", SqlDbType.Int).Value = stockTransaction.DeliveryLocationID ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@Status", SqlDbType.TinyInt).Value = stockTransaction.Status ?? (object)DBNull.Value;  // TinyInt for tinyint
            command.Parameters.Add("@StockTransactionTypeID", SqlDbType.Int).Value = stockTransaction.StockTransactionTypeID ?? (object)DBNull.Value;
            command.Parameters.Add("@TransactionDate", SqlDbType.DateTime2).Value = stockTransaction.TransactionDate ?? (object)DBNull.Value;  // FIXED: Column name
            command.Parameters.Add("@UpdateDate", SqlDbType.DateTime2).Value = stockTransaction.UpdateDate ?? DateTime.Now;  // FIXED: Column name
            command.Parameters.Add("@TotalQuantity", SqlDbType.Decimal).Value = stockTransaction.TotalQuantity ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = stockTransaction.TotalAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@SupplierInvoiceNumber", SqlDbType.NVarChar).Value = stockTransaction.SupplierInvoiceNumber ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@DeliveryDate", SqlDbType.DateTime2).Value = stockTransaction.DeliveryDate ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = stockTransaction.Note ?? (object)DBNull.Value;
            command.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = stockTransaction.SubTotal ?? (object)DBNull.Value;  // FIXED: Lowercase 's'
            command.Parameters.Add("@GrossAmount", SqlDbType.Decimal).Value = stockTransaction.GrossAmount ?? (object)DBNull.Value;  // FIXED: Lowercase 's'
            command.Parameters.Add("@TotalRemainingAmount", SqlDbType.Decimal).Value = stockTransaction.TotalRemainingAmount ?? (object)DBNull.Value;  // FIXED: Lowercase 's'
            command.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = stockTransaction.DiscountAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountPerc", SqlDbType.Real).Value = stockTransaction.DiscountPerc ?? (object)DBNull.Value;  // FIXED: Real type
            command.Parameters.Add("@ShippingCost", SqlDbType.Decimal).Value = stockTransaction.ShippingCost ?? (object)DBNull.Value;  // ADDED
            command.Parameters.Add("@ImportDuty", SqlDbType.Decimal).Value = stockTransaction.ImportDuty ?? (object)DBNull.Value;
            command.Parameters.Add("@RouteID", SqlDbType.Int).Value = stockTransaction.RouteID ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = stockTransaction.CreatedByUserID ?? (object)DBNull.Value;  // FIXED: Int not NVarChar
            command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = stockTransaction.LastModifiedUtc ?? (object)DBNull.Value;

        }
    }

    
}

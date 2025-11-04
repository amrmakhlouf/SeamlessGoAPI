using SeamlessGo.Models;
using System.Data.SqlClient;
using System.Data;

namespace SeamlessGo.Data
{
    public class StockTransactionLineRepository : IStockTransactionLineRepository
    {
        private readonly string _connectionString;

        public StockTransactionLineRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }

        public async Task<IEnumerable<StockTransactionLine>> GetByTransactionIdAsync(string StockTransactionId)
        {
            var StockTransactionLines = new List<StockTransactionLine>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.StockTransactionLines WHERE StockTransactionID = @StockTransactionID Order BY StockTransactionLineID",
                connection);

            command.Parameters.Add("@StockTransactionID", SqlDbType.VarChar).Value = StockTransactionId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                StockTransactionLines.Add(MapReaderToStockTransactionLine(reader));
            }

            return StockTransactionLines;
        }

        public async Task<StockTransactionLine> CreateAsync(StockTransactionLine StockTransactionLine)
        {

            //

            const string sql = @"
                INSERT INTO dbo.StockTransactionLines 
                (StockTransactionLineID,StockTransactionID, ItemPackID, DiscountAmount, DiscountPerc, Bonus, Quantity, ItemPrice, FullPrice, TotalPrice, Note)
                VALUES 
                (@StockTransactionLineID,@StockTransactionID, @ItemPackID, @DiscountAmount, @DiscountPerc, @Bonus, @Quantity, @ItemPrice, @FullPrice, @TotalPrice, @Note);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddStockTransactionLineParameters(command, StockTransactionLine);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return StockTransactionLine;
        }



        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.StockTransactionLines WHERE StockTransactionLineID = @StockTransactionLineID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@StockTransactionLineID", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }



        private static StockTransactionLine MapReaderToStockTransactionLine(SqlDataReader reader)
        {
            return new StockTransactionLine
            {
                StockTransactionLineID = reader.IsDBNull("StockTransactionLineID") ? null : reader.GetString("StockTransactionLineID"),

                // StockTransactionLineID = reader.GetInt32("StockTransactionLineID"),
                StockTransactionID = reader.IsDBNull("StockTransactionID") ? null : reader.GetString("StockTransactionID"),
                ItemPackID = reader.IsDBNull("ItemPackID") ? null : reader.GetGuid("ItemPackID").ToString(),
                Quantity = reader.IsDBNull("Quantity") ? null : reader.GetInt32("Quantity"),

                Coast = reader.IsDBNull("Coast") ? null : reader.GetDecimal("Coast"),
                TotalCost = reader.IsDBNull("TotalCost") ? null : reader.GetDecimal("TotalCost"),

                ExpirationDate = reader.IsDBNull("ExpirationDate") ? null : reader.GetDateTime("ExpirationDate"),
              
            };
        }

        private static void AddStockTransactionLineParameters(SqlCommand command, StockTransactionLine line)
        {
            command.Parameters.Add("@StockTransactionLineID", SqlDbType.NVarChar, 450).Value =
       line.StockTransactionLineID ?? (object)DBNull.Value;

            command.Parameters.Add("@StockTransactionID", SqlDbType.Int).Value = line.StockTransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPackID", SqlDbType.UniqueIdentifier).Value = line.ItemPackID ?? (object)DBNull.Value;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = line.Quantity ?? (object)DBNull.Value;

            command.Parameters.Add("@Coast", SqlDbType.Decimal).Value = line.Coast ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalCost", SqlDbType.Decimal).Value = line.TotalCost ?? (object)DBNull.Value;

            command.Parameters.Add("@DiscountPerc", SqlDbType.DateTime).Value = line.ExpirationDate ?? (object)DBNull.Value;
            
        }
    }
}

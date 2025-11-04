using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class TransactionLineRepository : ITransactionLineRepository
    {
        private readonly string _connectionString;

        public TransactionLineRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }

        public async Task<IEnumerable<TransactionLine>> GetByTransactionIdAsync(string TransactionId)
        {
            var TransactionLines = new List<TransactionLine>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.TransactionLines WHERE TransactionID = @TransactionID Order BY TransactionLineID",
                connection);

            command.Parameters.Add("@TransactionID", SqlDbType.VarChar).Value = TransactionId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                TransactionLines.Add(MapReaderToTransactionLine(reader));
            }

            return TransactionLines;
        }

        public async Task<TransactionLine> CreateAsync(TransactionLine TransactionLine)
        {

            //

            const string sql = @"
                INSERT INTO dbo.TransactionLines 
                (TransactionLineID,TransactionID, ItemPackID, DiscountAmount, DiscountPerc, Bonus, Quantity, ItemPrice, FullPrice, TotalPrice, Note)
                VALUES 
                (@TransactionLineID,@TransactionID, @ItemPackID, @DiscountAmount, @DiscountPerc, @Bonus, @Quantity, @ItemPrice, @FullPrice, @TotalPrice, @Note);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddTransactionLineParameters(command, TransactionLine);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return TransactionLine;
        }



        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.TransactionLines WHERE TransactionLineID = @TransactionLineID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@TransactionLineID", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }



        private static TransactionLine MapReaderToTransactionLine(SqlDataReader reader)
        {
            return new TransactionLine
            {
                TransactionLineID = reader.IsDBNull("TransactionLineID") ? null : reader.GetString("TransactionLineID"),

                // TransactionLineID = reader.GetInt32("TransactionLineID"),
                TransactionID = reader.IsDBNull("TransactionID") ? null : reader.GetString("TransactionID"),
                ItemPackID = reader.IsDBNull("ItemPackID") ? null : reader.GetGuid("ItemPackID").ToString(),

                DiscountAmount = reader.IsDBNull("DiscountAmount") ? null : reader.GetDecimal("DiscountAmount"),
                DiscountPerc = reader.IsDBNull("DiscountPerc") ? null : (float)reader.GetDecimal("DiscountPerc"),
                Bonus = reader.IsDBNull("Bonus") ? null : reader.GetDecimal("Bonus"),
                Quantity = reader.IsDBNull("Quantity") ? null : reader.GetInt32("Quantity"),
                ItemPrice = reader.IsDBNull("ItemPrice") ? null : reader.GetDecimal("ItemPrice"),
                FullPrice = reader.IsDBNull("FullPrice") ? null : reader.GetDecimal("FullPrice"),
                TotalPrice = reader.IsDBNull("TotalPrice") ? null : reader.GetDecimal("TotalPrice"),
                Note = reader.IsDBNull("Note") ? null : reader.GetString("Note")
            };
        }

        private static void AddTransactionLineParameters(SqlCommand command, TransactionLine line)
        {
            command.Parameters.Add("@TransactionLineID", SqlDbType.NVarChar, 450).Value =
       line.TransactionLineID ?? (object)DBNull.Value;

            command.Parameters.Add("@TransactionID", SqlDbType.Int).Value = line.TransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPackID", SqlDbType.UniqueIdentifier).Value = line.ItemPackID ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = line.DiscountAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountPerc", SqlDbType.Float).Value = line.DiscountPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@Bonus", SqlDbType.Int).Value = line.Bonus ?? (object)DBNull.Value;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = line.Quantity ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemPrice", SqlDbType.Decimal).Value = line.ItemPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@FullPrice", SqlDbType.Decimal).Value = line.FullPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalPrice", SqlDbType.Decimal).Value = line.TotalPrice ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = line.Note ?? (object)DBNull.Value;
        }
    }
}

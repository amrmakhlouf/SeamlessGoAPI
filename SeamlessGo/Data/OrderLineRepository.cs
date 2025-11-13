using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class OrderLineRepository : IOrderLineRepository
    {
        private readonly string _connectionString;

        public OrderLineRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }

        public async Task<IEnumerable<OrderLine>> GetByOrderIdAsync(string orderId)
        {
            var orderLines = new List<OrderLine>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.OrderLines WHERE OrderID = @OrderID ORDER BY OrderLineID",
                connection);

            command.Parameters.Add("@OrderID", SqlDbType.NVarChar).Value = orderId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orderLines.Add(MapReaderToOrderLine(reader));
            }

            return orderLines;
        }

        public async Task<OrderLine> CreateAsync(OrderLine orderLine)
        {

        //

            const string sql = @"
                INSERT INTO dbo.OrderLines 
                (OrderLineID,OrderID, ItemPackID, DiscountAmount, DiscountPerc, Bonus, Quantity, ItemPrice, FullPrice, TotalPrice, Note)
                VALUES 
                (@OrderLineID,@OrderID, @ItemPackID, @DiscountAmount, @DiscountPerc, @Bonus, @Quantity, @ItemPrice, @FullPrice, @TotalPrice, @Note);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddOrderLineParameters(command, orderLine);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return orderLine;
        }

      

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.OrderLines WHERE OrderLineID = @OrderLineID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@OrderLineID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

       

        private static OrderLine MapReaderToOrderLine(SqlDataReader reader)
        {
            return new OrderLine
            {
                OrderLineID = reader.IsDBNull("OrderLineID") ? null : reader.GetString("OrderLineID"),

               // OrderLineID = reader.GetInt32("OrderLineID"),
                OrderID = reader.IsDBNull("OrderID") ? null : reader.GetString("OrderID"),
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

        private static void AddOrderLineParameters(SqlCommand command, OrderLine line)
        {
            command.Parameters.Add("@OrderLineID", SqlDbType.NVarChar, 450).Value =
       line.OrderLineID ?? (object)DBNull.Value;

            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = line.OrderID ?? (object)DBNull.Value;
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

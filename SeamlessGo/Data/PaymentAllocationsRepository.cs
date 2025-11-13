using SeamlessGo.Models;
using System.Data.SqlClient;
using System.Data;

namespace SeamlessGo.Data
{
    public class PaymentAllocationsRepository : IPaymentAllocationsRepository
    {
        private readonly string _connectionString;

        public PaymentAllocationsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }
        public async Task<IEnumerable<PaymentAllocations>> GetByOrderIdAsync(string paymentid)
        {
            var allocations = new List<PaymentAllocations>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.paymentallocations WHERE paymentID = @paymentID ORDER BY OrderLineID",
                connection);

            command.Parameters.Add("@paymentID", SqlDbType.NVarChar).Value = paymentid;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                allocations.Add(MapReaderToOrderLine(reader));
            }

            return allocations;
        }

        public async Task<PaymentAllocations> CreateAsync(PaymentAllocations allocation)
        {

            //
            const string sql = @"
    INSERT INTO dbo.PaymentAllocations 
    (PaymentID, TransactionID, AllocatedAmount, SyncStatus)
    VALUES 
    (@PaymentID, @TransactionID, @AllocatedAmount, @SyncStatus);
";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddOrderLineParameters(command, allocation);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return allocation;
        }
        private static PaymentAllocations MapReaderToOrderLine(SqlDataReader reader)
        {
            return new PaymentAllocations
            {
                PaymentID = reader.IsDBNull("PaymentID") ? null : reader.GetString("PaymentID"),
                TransactionID = reader.IsDBNull("TransactionID") ? null : reader.GetString("TransactionID"),
                AllocatedAmount = reader.IsDBNull("AllocatedAmount") ? 0 : reader.GetDecimal("AllocatedAmount"),
                SyncStatus = reader.IsDBNull("SyncStatus") ? 0 : reader.GetInt32("SyncStatus")
            };
        }

        private static void AddOrderLineParameters(SqlCommand command, PaymentAllocations allocation)
        {
            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = allocation.PaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = allocation.TransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@AllocatedAmount", SqlDbType.Decimal).Value = allocation.AllocatedAmount;
            command.Parameters.Add("@SyncStatus", SqlDbType.Int).Value = allocation.SyncStatus;
        }
    }

}


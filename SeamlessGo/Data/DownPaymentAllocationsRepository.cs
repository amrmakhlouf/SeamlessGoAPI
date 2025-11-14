using SeamlessGo.Models;
using System.Data.SqlClient;
using System.Data;

namespace SeamlessGo.Data
{
    public class DownPaymentAllocationsRepository: IDownPaymentAllocationsRepository
    {
        private readonly string _connectionString;

        public DownPaymentAllocationsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }
        public async Task<IEnumerable<DownPaymentAllocations>> GetByOrderIdAsync(string paymentid)
        {
            var allocations = new List<DownPaymentAllocations>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.DownPaymentAllocations WHERE DownPaymentID = @DownPaymentID ORDER BY DownPaymentID",
                connection);

            command.Parameters.Add("@DownPaymentID", SqlDbType.NVarChar).Value = paymentid;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                allocations.Add(MapReaderToOrderLine(reader));
            }

            return allocations;
        }

        public async Task<DownPaymentAllocations> CreateAsync(DownPaymentAllocations allocation)
        {

            //
            const string sql = @"
    INSERT INTO dbo.DownPaymentAllocations 
    (PaymentID, DownPaymentID, AllocatedAmount, SyncStatus)
    VALUES 
    (@PaymentID, @DownPaymentID, @AllocatedAmount, @SyncStatus);
";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddOrderLineParameters(command, allocation);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return allocation;
        }
        private static DownPaymentAllocations MapReaderToOrderLine(SqlDataReader reader)
        {
            return new DownPaymentAllocations
            {
                PaymentID = reader.IsDBNull("PaymentID") ? null : reader.GetString("PaymentID"),
                DownPaymentID = reader.IsDBNull("DownPaymentID") ? null : reader.GetString("DownPaymentID"),
                AllocatedAmount = reader.IsDBNull("AllocatedAmount") ? null : reader.GetString("AllocatedAmount"),
            };
        }

        private static void AddOrderLineParameters(SqlCommand command, DownPaymentAllocations allocation)
        {
            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = allocation.PaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@DownPaymentID", SqlDbType.NVarChar).Value = allocation.DownPaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@AllocatedAmount", SqlDbType.NVarChar).Value = allocation.AllocatedAmount ?? (object)DBNull.Value;
        }
    }
}


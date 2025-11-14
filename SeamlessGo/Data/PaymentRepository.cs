using System.Data;
using System.Data.SqlClient;
using SeamlessGo.DTOs;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class PaymentRepository : IPaymentsRepository
    {
        private readonly string _connectionString;
        private readonly IPaymentAllocationsRepository _paymentallocationrepository;
        public PaymentRepository(IConfiguration configuration, IPaymentAllocationsRepository paymnetRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _paymentallocationrepository = paymnetRepository;
        }

        public async Task<IEnumerable<Payments>> GetAllAsync(DateTime? LastModifiedUtc)
        {
            var payments = new List<Payments>();

            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM dbo.Payments where 1=1";
            if (LastModifiedUtc.HasValue)
            {
                query += "And LastModifiedUtc > @LastModifiedUtc ";
            }
            query += " ORDER BY CreatedDate DESC";

            using var command = new SqlCommand(query, connection);
            if (LastModifiedUtc.HasValue )
            {
                command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = LastModifiedUtc;
            }
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(MapReaderToOrder(reader));
            }

            return payments;
        }

        public async Task<Payments?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Payments WHERE PaymentID = @PaymentID", connection);

            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToOrder(reader);
            }

            return null;
        }

        public async Task<Payments?> GetByIdWithLinesAsync(string id)
        {
            var payment = await GetByIdAsync(id);

            if (payment != null)
            {
                payment.Allocations = (await _paymentallocationrepository.GetByOrderIdAsync(id)).ToList();
            }

            return payment;
        }

        public async Task<Payments> CreateAsync(Payments payment, List<PaymentAllocations>? paymentallocation)
        {
            const string sql = @"
    INSERT INTO dbo.Payments 
    (PaymentID, Amount, ChequeID, CreatedByUserID, CreatedDate, CurrencyID, CustomerID, 
     PaymentMethod, PaymentStatus, RouteID,  UpdatedDate, IsVoided, ClientID, LastModifiedUtc)
    VALUES 
    (@PaymentID, @Amount, @ChequeID, @CreatedByUserID, @CreatedDate, @CurrencyID, @CustomerID,
     @PaymentMethod, @PaymentStatus, @RouteID,  @UpdatedDate, @IsVoided, @ClientID, @LastModifiedUtc);
";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert order with user-provided OrderID
                using var command = new SqlCommand(sql, connection, transaction);
                AddOrderParameters(command, payment);

                // Execute INSERT - no need to get SCOPE_IDENTITY
                await command.ExecuteNonQueryAsync();

                // Use the OrderID that user provided (already in order.OrderID)
                string newOrderId = payment.PaymentID;

                // Insert order lines if provided
                if (paymentallocation != null && paymentallocation.Any())
                {
                    foreach (var line in paymentallocation)
                    {
                        line.PaymentID = newOrderId;  // Assign the user-provided OrderID
                        await CreatePaymentAllocationAsync(connection, transaction, line);
                    }
                }
                transaction.Commit();
                return payment;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        //public async Task<IEnumerable<Orders>> GetOrdersByCustomerAsync(string customerId)
        //{
        //    var orders = new List<Orders>();

        //    const string sql = "SELECT * FROM dbo.Orders WHERE CustomerID = @CustomerID ORDER BY OrderDate DESC";

        //    using var connection = new SqlConnection(_connectionString);
        //    using var command = new SqlCommand(sql, connection);

        //    command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = customerId;

        //    await connection.OpenAsync();
        //    using var reader = await command.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        orders.Add(MapReaderToOrder(reader));
        //    }

        //    return orders;
        //}

        // Private helper method - not part of interface
        // Used internally by CreateAsync to insert order lines
        private async Task CreatePaymentAllocationAsync(SqlConnection connection, SqlTransaction transaction, PaymentAllocations allocation)
        {

            const string sql = @"
    INSERT INTO dbo.PaymentAllocations 
    (PaymentID, TransactionID, AllocatedAmount )
    VALUES 
    (@PaymentID, @TransactionID, @AllocatedAmount );
";

            using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = allocation.PaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = allocation.TransactionID ?? (object)DBNull.Value;
            command.Parameters.Add("@AllocatedAmount", SqlDbType.Decimal).Value = allocation.AllocatedAmount ?? (object)DBNull.Value;

            await command.ExecuteScalarAsync();



        }

        private static Payments MapReaderToOrder(SqlDataReader reader)
        {
            return new Payments
            {

                PaymentID = reader.IsDBNull("PaymentID") ? null : reader.GetString("PaymentID"),
                Amount = reader.IsDBNull("Amount") ? 0 : reader.GetDecimal("Amount"),
                ChequeID = reader.IsDBNull("ChequeID") ? null : (int?)reader.GetInt32("ChequeID"),
                CreatedByUserID = reader.IsDBNull("CreatedByUserID") ? 0 : reader.GetInt32("CreatedByUserID"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                CurrencyID = reader.IsDBNull("CurrencyID") ? 0 : reader.GetInt32("CurrencyID"),
                CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                PaymentMethod = reader.IsDBNull("PaymentMethod") ? 0 : reader.GetInt32("PaymentMethod"),
                PaymentStatus = reader.IsDBNull("PaymentStatus") ? 0 : reader.GetInt32("PaymentStatus"),
                RouteID = reader.IsDBNull("RouteID") ? 0 : reader.GetInt32("RouteID"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : (DateTime?)reader.GetDateTime("UpdatedDate"),
                IsVoided = reader.IsDBNull("IsVoided") ? false : reader.GetBoolean("IsVoided"),
                ClientID = reader.IsDBNull("ClientID") ? Guid.Empty : reader.GetGuid("ClientID"),
                LastModifiedUtc = reader.IsDBNull("LastModifiedUtc") ? DateTime.MinValue : reader.GetDateTime("LastModifiedUtc")
            };
        }
        private static void AddOrderParameters(SqlCommand command, Payments payment)

        {
            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = payment.PaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = payment.Amount ?? (object)DBNull.Value;
            command.Parameters.Add("@ChequeID", SqlDbType.Int).Value = payment.ChequeID ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = payment.CreatedByUserID;
            command.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = payment.CreatedDate;
            command.Parameters.Add("@CurrencyID", SqlDbType.Int).Value = payment.CurrencyID?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = payment.CustomerID ?? (object)DBNull.Value;
            command.Parameters.Add("@PaymentMethod", SqlDbType.Int).Value = payment.PaymentMethod?? (object)DBNull.Value;
            command.Parameters.Add("@PaymentStatus", SqlDbType.Int).Value = payment.PaymentStatus?? (object)DBNull.Value;
            command.Parameters.Add("@RouteID", SqlDbType.Int).Value = payment.RouteID?? (object)DBNull.Value;
            command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime2).Value = payment.UpdatedDate ?? (object)DBNull.Value;
            command.Parameters.Add("@IsVoided", SqlDbType.Bit).Value = payment.IsVoided ?? (object)DBNull.Value;
            command.Parameters.Add("@ClientID", SqlDbType.UniqueIdentifier).Value = payment.ClientID?? (object)DBNull.Value;
            command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = payment.LastModifiedUtc ?? (object)DBNull.Value;
        }
    }
}

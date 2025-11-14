using SeamlessGo.Models;
using System.Data.SqlClient;
using System.Data;
using System.Security.AccessControl;

namespace SeamlessGo.Data
{
    public class DownPaymentRepository : IDownPaymentsRepository 
    {
        private readonly string _connectionString;
        private readonly IDownPaymentAllocationsRepository _paymentallocationrepository;
        public DownPaymentRepository(IConfiguration configuration, IDownPaymentAllocationsRepository paymnetRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _paymentallocationrepository = paymnetRepository;
        }

        public async Task<IEnumerable<DownPayment>> GetAllAsync(DateTime? LastModifiedUtc)
        {
            var DownPayments = new List<DownPayment>();

            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM dbo.DownPayments where 1=1";
            if (LastModifiedUtc.HasValue)
            {
                query += "And LastModifiedUtc > @LastModifiedUtc ";
            }
            query += " ORDER BY CreatedDate DESC";

            using var command = new SqlCommand(query, connection);
            if (LastModifiedUtc.HasValue)
            {
                command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = LastModifiedUtc;
            }

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                DownPayments.Add(MapReaderToOrder(reader));
            }

            return DownPayments;
        }

        public async Task<DownPayment?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.DownPayments WHERE DownPaymentID = @DownPaymentID", connection);

            command.Parameters.Add("@DownPaymentID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToOrder(reader);
            }

            return null;
        }

        public async Task<DownPayment?> GetByIdWithLinesAsync(string id)
        {
            var payment = await GetByIdAsync(id);

            if (payment != null)
            {
                payment.Allocations = (await _paymentallocationrepository.GetByOrderIdAsync(id)).ToList();
            }

            return payment;
        }

        public async Task<DownPayment> CreateAsync(DownPayment downpayment, List<DownPaymentAllocations>? paymentallocation)
        {
            const string sql = @"
    INSERT INTO dbo.DownPayments 
    (DownPaymentID, Amount, ChequeID, CreatedByUserID, CreatedDate, CreatedFromPaymentID, CurrencyID, 
     CustomerID, PaymentMethod, PaymentStatus, RemainingAmount, RouteID,  UpdatedDate, 
     IsVoided, ClientID, LastModifiedUtc)
    VALUES 
    (@DownPaymentID, @Amount, @ChequeID, @CreatedByUserID, @CreatedDate, @CreatedFromPaymentID, @CurrencyID,
     @CustomerID, @PaymentMethod, @PaymentStatus, @RemainingAmount, @RouteID,  @UpdatedDate,
     @IsVoided, @ClientID, @LastModifiedUtc);
";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert order with user-provided OrderID
                using var command = new SqlCommand(sql, connection, transaction);
                AddDownPaymentrParameters(command, downpayment);

                // Execute INSERT - no need to get SCOPE_IDENTITY
                await command.ExecuteNonQueryAsync();

                // Use the OrderID that user provided (already in order.OrderID)
                string newOrderId = downpayment.DownPaymentID;

                // Insert order lines if provided
                if (paymentallocation != null && paymentallocation.Any())
                {
                    foreach (var line in paymentallocation)
                    {
                        line.DownPaymentID = newOrderId;  // Assign the user-provided OrderID
                        await CreateDownPaymentAllocationAsync(connection, transaction, line);
                    }
                }
                transaction.Commit();
                return downpayment;
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
        private async Task CreateDownPaymentAllocationAsync(SqlConnection connection, SqlTransaction transaction, DownPaymentAllocations allocation)
        {

            const string sql = @"
    INSERT INTO dbo.DownPaymentAllocations 
    (PaymentID, DownPaymentID, AllocatedAmount )
    VALUES 
    (@PaymentID, @DownPaymentID, @AllocatedAmount);
";

            using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@PaymentID", SqlDbType.NVarChar).Value = allocation.PaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@DownPaymentID", SqlDbType.NVarChar).Value = allocation.DownPaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@AllocatedAmount", SqlDbType.NVarChar).Value = allocation.AllocatedAmount ?? (object)DBNull.Value;

            await command.ExecuteScalarAsync();



        }

        private static DownPayment MapReaderToOrder(SqlDataReader reader)
        {
            return new DownPayment
            {

                DownPaymentID = reader.IsDBNull("DownPaymentID") ? null : reader.GetString("DownPaymentID"),
                Amount = reader.IsDBNull("Amount") ? null : reader.GetString("Amount"),
                ChequeID = reader.IsDBNull("ChequeID") ? null : (int?)reader.GetInt32("ChequeID"),
                CreatedByUserID = reader.IsDBNull("CreatedByUserID") ? 0 : reader.GetInt32("CreatedByUserID"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? DateTime.MinValue : reader.GetDateTime("CreatedDate"),
                CreatedFromPaymentID = reader.IsDBNull("CreatedFromPaymentID") ? null : reader.GetString("CreatedFromPaymentID"),
                CurrencyID = reader.IsDBNull("CurrencyID") ? 0 : reader.GetInt32("CurrencyID"),
                CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                PaymentMethod = reader.IsDBNull("PaymentMethod") ? 0 : reader.GetInt32("PaymentMethod"),
                PaymentStatus = reader.IsDBNull("PaymentStatus") ? 0 : reader.GetInt32("PaymentStatus"),
                RemainingAmount = reader.IsDBNull("RemainingAmount") ? null : reader.GetString("RemainingAmount"),
                RouteID = reader.IsDBNull("RouteID") ? 0 : reader.GetInt32("RouteID"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : (DateTime?)reader.GetDateTime("UpdatedDate"),
                IsVoided = reader.IsDBNull("IsVoided") ? false : reader.GetBoolean("IsVoided"),
                ClientID = reader.IsDBNull("ClientID") ? Guid.Empty : reader.GetGuid("ClientID"),
                LastModifiedUtc = reader.IsDBNull("LastModifiedUtc") ? DateTime.MinValue : reader.GetDateTime("LastModifiedUtc")
            };
        }
        private static void AddDownPaymentrParameters(SqlCommand command, DownPayment downPayment)

        {
            command.Parameters.Add("@DownPaymentID", SqlDbType.NVarChar).Value = downPayment.DownPaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@Amount", SqlDbType.NVarChar).Value = downPayment.Amount ?? (object)DBNull.Value;
            command.Parameters.Add("@ChequeID", SqlDbType.Int).Value = downPayment.ChequeID ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = downPayment.CreatedByUserID ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = downPayment.CreatedDate ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedFromPaymentID", SqlDbType.NVarChar).Value = downPayment.CreatedFromPaymentID ?? (object)DBNull.Value;
            command.Parameters.Add("@CurrencyID", SqlDbType.Int).Value = downPayment.CurrencyID ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = downPayment.CustomerID ?? (object)DBNull.Value;
            command.Parameters.Add("@PaymentMethod", SqlDbType.Int).Value = downPayment.PaymentMethod ?? (object)DBNull.Value;
            command.Parameters.Add("@PaymentStatus", SqlDbType.Int).Value = downPayment.PaymentStatus ?? (object)DBNull.Value;
            command.Parameters.Add("@RemainingAmount", SqlDbType.NVarChar).Value = downPayment.RemainingAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@RouteID", SqlDbType.Int).Value = downPayment.RouteID ?? (object)DBNull.Value;
            command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime2).Value = downPayment.UpdatedDate ?? (object)DBNull.Value;
            command.Parameters.Add("@IsVoided", SqlDbType.Bit).Value = downPayment.IsVoided ?? (object)DBNull.Value;
            command.Parameters.Add("@ClientID", SqlDbType.UniqueIdentifier).Value = downPayment.ClientID ?? (object)DBNull.Value;
            command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = downPayment.LastModifiedUtc ?? (object)DBNull.Value;
        }
    }
}


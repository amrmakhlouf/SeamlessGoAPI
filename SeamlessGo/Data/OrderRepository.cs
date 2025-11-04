using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly IOrderLineRepository _orderLineRepository;

        public OrderRepository(IConfiguration configuration, IOrderLineRepository orderLineRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _orderLineRepository = orderLineRepository;
        }

        public async Task<IEnumerable<Orders>> GetAllAsync()
        {
            var orders = new List<Orders>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Orders ORDER BY OrderID DESC", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(MapReaderToOrder(reader));
            }

            return orders;
        }

        public async Task<Orders?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Orders WHERE OrderID = @OrderID", connection);

            command.Parameters.Add("@OrderID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToOrder(reader);
            }

            return null;
        }

        public async Task<Orders?> GetByIdWithLinesAsync(string id)
        {
            var order = await GetByIdAsync(id);

            if (order != null)
            {
                order.OrderLines = (await _orderLineRepository.GetByOrderIdAsync(id)).ToList();
            }

            return order;
        }

        public async Task<Orders> CreateAsync(Orders order, List<OrderLines>? orderLines)
        {
            const string sql = @"
                INSERT INTO dbo.Orders 
                (OrderID,CustomerID, OrderDate, UpdatedDate, OrderTypeID, SubTotal, TotalAmount, GrossAmount, 
                 TotalRemainingAmount, DiscountAmount, DiscountPerc, NetAmount, Tax, TaxPerc, 
                 Status, CreatedByUserID, RouteID, IsVoided, Note, SourceOrderID,SyncStatus)
                VALUES 
                (@OrderID,@CustomerID, @OrderDate, @UpdateDate, @OrderTypeID,@SubTotal, @TotalAmount, @GrossAmount,
                 @TotalRemainingAmount, @DiscountAmount, @DiscountPerc, @NetAmount, @Tax, @TaxPerc,
                 @Status, @CreatedByUserID, @RouteID, @IsVoided, @Note, null,@SyncStatus);
                ";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert order with user-provided OrderID
                using var command = new SqlCommand(sql, connection, transaction);
                AddOrderParameters(command, order);

                // Execute INSERT - no need to get SCOPE_IDENTITY
                await command.ExecuteNonQueryAsync();

                // Use the OrderID that user provided (already in order.OrderID)
                string newOrderId = order.OrderID;

                // Insert order lines if provided
                if (orderLines != null && orderLines.Any())
                {
                    foreach (var line in orderLines)
                    {
                        line.OrderID = newOrderId;  // Assign the user-provided OrderID
                        await CreateOrderLineAsync(connection, transaction, line);
                    }
                }
                transaction.Commit();
                return order;
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
        private async Task CreateOrderLineAsync(SqlConnection connection, SqlTransaction transaction, OrderLines line)
        {

            const string sql = @"
                INSERT INTO dbo.OrderLines 
                (OrderLineID,OrderID, ItemPackID, DiscountAmount, DiscountPerc, Bonus, Quantity, ItemPrice, FullPrice, TotalPrice, Note)
                VALUES 
                (@OrderLineID,@OrderID, @ItemPackID, @DiscountAmount, @DiscountPerc, @Bonus, @Quantity, @ItemPrice, @FullPrice, 0, @Note);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@OrderLineID", SqlDbType.NVarChar).Value = line.OrderLineID;

            command.Parameters.Add("@OrderID", SqlDbType.NVarChar).Value = line.OrderID ?? (object)DBNull.Value;
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

        private static Orders MapReaderToOrder(SqlDataReader reader)
        {
            return new Orders
            {

                OrderID = reader.IsDBNull("OrderID") ? null : reader.GetString("OrderID"),
                CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetGuid("CustomerID").ToString(),
                OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                UpdateDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                OrderTypeID = reader.IsDBNull("OrderTypeID") ? null : reader.GetInt32("OrderTypeID"),
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
                InvoicedID = reader.IsDBNull("sourceorderid") ? null : reader.GetString("sourceorderid"),


                // ✅ tinyint -> byte -> int
                SyncStatus = reader.IsDBNull("SyncStatus") ? null : (int?)reader.GetByte("SyncStatus")
            };  
          }
        private static void AddOrderParameters(SqlCommand command, Orders order)

        {
            command.Parameters.Add("@OrderID", SqlDbType.NVarChar).Value = order.OrderID ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = order.CustomerID ?? (object)DBNull.Value;
            command.Parameters.Add("@OrderDate", SqlDbType.DateTime2).Value = order.OrderDate ?? (object)DBNull.Value;
            command.Parameters.Add("@UpdateDate", SqlDbType.DateTime2).Value = DateTime.Now;
            command.Parameters.Add("@OrderTypeID", SqlDbType.Int).Value = order.OrderTypeID ?? (object)DBNull.Value;
            command.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = order.SubTotal ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = order.TotalAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@GrossAmount", SqlDbType.Decimal).Value = order.GrossAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@TotalRemainingAmount", SqlDbType.Decimal).Value = order.TotalRemainingAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = order.DiscountAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@DiscountPerc", SqlDbType.Float).Value = order.DiscountPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = order.NetAmount ?? (object)DBNull.Value;
            command.Parameters.Add("@Tax", SqlDbType.Decimal).Value = order.Tax ?? (object)DBNull.Value;
            command.Parameters.Add("@TaxPerc", SqlDbType.Float).Value = order.TaxPerc ?? (object)DBNull.Value;
            command.Parameters.Add("@Status", SqlDbType.TinyInt).Value = order.Status ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedByUserID", SqlDbType.NVarChar).Value = order.CreatedByUserID ?? (object)DBNull.Value;
            command.Parameters.Add("@RouteID", SqlDbType.Int).Value = order.RouteID ?? (object)DBNull.Value;
            command.Parameters.Add("@IsVoided", SqlDbType.Bit).Value = order.IsVoided ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = order.Note ?? (object)DBNull.Value;
            command.Parameters.Add("@InvoicedID", SqlDbType.NVarChar).Value = order.InvoicedID ?? (object)DBNull.Value;
            command.Parameters.Add("@SyncStatus", SqlDbType.NVarChar).Value = order.SyncStatus ?? (object)DBNull.Value;
        }
    }
}


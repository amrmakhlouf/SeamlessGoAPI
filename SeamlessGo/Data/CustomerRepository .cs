

using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data


{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;
        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var customers = new List<Customer>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Customers ORDER BY CustomerID", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(MapReaderToCustomer(reader));
            }

            return customers;
        }

        public async Task<Customer?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Customers WHERE CustomerID = @CustomerID", connection);

            command.Parameters.Add("@CustomerID", SqlDbType.VarChar).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToCustomer(reader);
            }

            return null;
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            try
            {
                const string sql = @"
INSERT INTO dbo.Customers 
(CustomerID, CustomerCode, FullName,  CityID, Address, Email, PhoneNumber1, PhoneNumber2, Latitude,Longitude,
 CustomerTypeID, CustomerBalance, AccountLimit, CustomerGroupID, CreatedByUserID, CreatedDate, IsActive, CustomerNote)
VALUES 
(@CustomerID, @CustomerCode, @FullName,  @City, @Address, @Email, @PhoneNumber1, @PhoneNumber2,@Latitude,@Longitude,
 @CustomerTypeID, @CustomerBalance, @AccountLimit, @CustomerGroupID, @UserID, @CreatedDate, @IsActive, @CustomerNote);";

                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(sql, connection);

                AddCustomerParameters(command, customer);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

              //  customer.CreatedDate = DateTime.Now;

                // No need to assign newId since CustomerID was already set
                return customer;
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"SQL Error: {ex.Message}");
                throw new Exception("An error occurred while creating the customer in the database.", ex);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An unexpected error occurred:");
                Console.Error.WriteLine($"Message: {ex.Message}");
                Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine("Inner Exception:");
                    Console.Error.WriteLine($"Message: {ex.InnerException.Message}");
                    Console.Error.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
                }

                throw;
            }
        }
       

        public async Task<bool> UpdateAsync(string id, Customer customer)
        {
            const string sql = @"
                UPDATE dbo.Customers 
                SET CustomerCode = @CustomerCode, FullName = @FullName,  
                    City = @City, Address = @Address, Email = @Email, PhoneNumber1 = @PhoneNumber1, 
                    PhoneNumber2 = @PhoneNumber2, CustomerTypeID = @CustomerTypeID, 
                    CustomerBalance = @CustomerBalance, AccountLimit = @AccountLimit, 
                    CustomerGroupID = @CustomerGroupID, UserID = @UserID, IsActive = @IsActive, Notes = @Notes
                WHERE CustomerID = @CustomerID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            AddCustomerParameters(command, customer);
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            const string sql = "DELETE FROM dbo.Customers WHERE CustomerID = @CustomerID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar).Value = id;

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? city)
        {
            var customers = new List<Customer>();
            var whereConditions = new List<string>();

            var sql = "SELECT * FROM dbo.Customers WHERE 1=1";

            if (!string.IsNullOrEmpty(name))
                whereConditions.Add("(FirstName LIKE @Name OR LastName LIKE @Name)");

            if (!string.IsNullOrEmpty(email))
                whereConditions.Add("Email LIKE @Email");

            if (!string.IsNullOrEmpty(city))
                whereConditions.Add("City LIKE @City");

            if (whereConditions.Any())
                sql += " AND " + string.Join(" AND ", whereConditions);

            sql += " ORDER BY CustomerID";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            if (!string.IsNullOrEmpty(name))
                command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = $"%{name}%";

            if (!string.IsNullOrEmpty(email))
                command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = $"%{email}%";

            if (!string.IsNullOrEmpty(city))
                command.Parameters.Add("@City", SqlDbType.NVarChar).Value = $"%{city}%";

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(MapReaderToCustomer(reader));
            }

            return customers;
        }

        private static Customer MapReaderToCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetGuid("CustomerID").ToString(),
                CustomerCode = reader.IsDBNull("CustomerCode") ? null : reader.GetString("CustomerCode"),
                FullName = reader.IsDBNull("FullName") ? null : reader.GetString("FullName"),
                CityID = reader.IsDBNull("CityID") ? null : reader.GetInt32("CityID"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                PhoneNumber1 = reader.IsDBNull("PhoneNumber1") ? null : reader.GetString("PhoneNumber1"),
                PhoneNumber2 = reader.IsDBNull("PhoneNumber2") ? null : reader.GetString("PhoneNumber2"),
                Latitude = reader.IsDBNull("Latitude") ? null : (decimal)reader.GetDecimal("Latitude"),
                Longitude = reader.IsDBNull("Longitude") ? null : (decimal)reader.GetDecimal("Longitude"),
                CustomerTypeID = reader.IsDBNull("CustomerTypeID") ? null : reader.GetInt32("CustomerTypeID"),
                CustomerBalance = reader.IsDBNull("CustomerBalance") ? null : reader.GetDecimal("CustomerBalance"),
                AccountLimit = reader.IsDBNull("AccountLimit") ? null : reader.GetDecimal("AccountLimit"),
                CustomerGroupID = reader.IsDBNull("CustomerGroupID") ? null : reader.GetInt32("CustomerGroupID"),
                CreatedByUserID = reader.IsDBNull("CreatedByUserID") ? null : reader.GetInt32("CreatedByUserID"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? null : reader.GetDateTime("CreatedDate"),
                IsActive = reader.IsDBNull("IsActive") ? null : reader.GetBoolean("IsActive"),
                CustomerNote = reader.IsDBNull("CustomerNote") ? null : reader.GetString("CustomerNote"),

            };
        }

        private static void AddCustomerParameters(SqlCommand command, Customer customer)
        {
            command.Parameters.Add("@CustomerID", SqlDbType.NVarChar, 50).Value = customer.CustomerID ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 50).Value = customer.CustomerCode ?? (object)DBNull.Value;
            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = customer.FullName ?? (object)DBNull.Value;
            command.Parameters.Add("@City", SqlDbType.Int, 100).Value = customer.CityID ?? (object)DBNull.Value;
            command.Parameters.Add("@Address", SqlDbType.NVarChar, 200).Value = customer.Address ?? (object)DBNull.Value;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = customer.Email ?? (object)DBNull.Value;
            command.Parameters.Add("@PhoneNumber1", SqlDbType.NVarChar, 20).Value = customer.PhoneNumber1 ?? (object)DBNull.Value;
            command.Parameters.Add("@PhoneNumber2", SqlDbType.NVarChar, 20).Value = customer.PhoneNumber2 ?? (object)DBNull.Value;
            command.Parameters.Add("@Latitude", SqlDbType.Decimal).Value = customer.Latitude ?? (object)DBNull.Value;
            command.Parameters.Add("@Longitude", SqlDbType.Decimal).Value = customer.Longitude ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerTypeID", SqlDbType.Int).Value = customer.CustomerTypeID ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerBalance", SqlDbType.Decimal).Value = customer.CustomerBalance ?? (object)DBNull.Value;
            command.Parameters.Add("@AccountLimit", SqlDbType.Decimal).Value = customer.AccountLimit ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerGroupID", SqlDbType.Int).Value = customer.CustomerGroupID ?? (object)DBNull.Value;
            command.Parameters.Add("@UserID", SqlDbType.Int, 50).Value = customer.CreatedByUserID ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = customer.CreatedDate ?? (object)DBNull.Value;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = customer.IsActive ?? (object)DBNull.Value;
            command.Parameters.Add("@CustomerNote", SqlDbType.NVarChar, -1).Value = customer.CustomerNote ?? (object)DBNull.Value;

        }
    }

}


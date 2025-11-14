using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class UsersRepository:IUsersRepository
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public UsersRepository(IConfiguration configuration, AppDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _context = context;

        }

        public async Task<IEnumerable<User>> GetAllAsync(string UserName, string Password)
        {

            var users = await _context.Users
                 .Include(u => u.client) 
                 .Include(u => u.stockLocation) 
                 .Where(u => u.UserName == UserName && u.Password == Password)
                 .ToListAsync();
            return users;

        }
        //private static Users MapReaderToOrder(SqlDataReader reader)
        //{
        //    return new Users
        //    {

        //        UserID = reader.IsDBNull("UserID") ? 0 : reader.GetInt32("UserID"),
        //        UserName = reader.IsDBNull("UserName") ? null : reader.GetString("UserName"),
        //        PhoneNumber = reader.IsDBNull("PhoneNumber") ? null : reader.GetString("PhoneNumber"),
        //        Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
        //        IsActive = reader.IsDBNull("IsActive") ? false : reader.GetBoolean("IsActive"),
        //        DisplayName = reader.IsDBNull("DisplayName") ? null : reader.GetString("DisplayName"),
        //        UserRoleID = reader.IsDBNull("UserRoleID") ? 0 : reader.GetInt32("UserRoleID")

        //    };
        //}
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class RouteRepository : IRouteRepository

    {

        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public RouteRepository(IConfiguration configuration, AppDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _context = context;
        }

        public async Task<SeamlessGo.Models.Route> GetAllAsync(int UserID)
        {
            var userExists = await _context.Users
      .AnyAsync(u => u.UserID == UserID);

            if (!userExists)
            {
                throw new ArgumentException($"User with ID {UserID} does not exist.");
            }
            // Insert new route
            var insertSql = @"
        INSERT INTO Routes (PlanID, UserID, StartDate, EndDate, Status)
        VALUES (1, @UserId, GETDATE(), NULL, 1)";
            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@UserId", UserID);
            await _context.Database.ExecuteSqlRawAsync(insertSql, parameter);

            var lastRoute = await _context.Routes
                .OrderByDescending(r => r.RouteID)
                .FirstOrDefaultAsync();

            return lastRoute;
        }
    }
}

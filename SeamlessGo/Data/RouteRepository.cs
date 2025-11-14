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

            var insertSql = @"
        INSERT INTO Routes (PlanID, UserID, StartDate, EndDate, Status)
        VALUES (1, @UserId, GETDATE(), NULL, 1)";
            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@UserId", UserID);
            await _context.Database.ExecuteSqlRawAsync(insertSql, parameter);

            var lastRoute = await _context.Routes
                .Include(r => r.Plan) // Include the Plan data
                .OrderByDescending(r => r.RouteID)
                .FirstOrDefaultAsync();

            return lastRoute;
        }

        public async Task<bool> UpdateStatusAsync(int RouteID, byte Status, DateTime EndDate)
        {
            var route = await _context.Routes.FindAsync(RouteID);
            if (route == null && route.Status !=1)
            {
                return false;
            }
            route.Status = Status;
            route.EndDate = EndDate;
            await _context.SaveChangesAsync();
            return true;
        }
        //public async Task<bool> CreateNewVisit(int RouteID)
        //{

            

        //}

    }
}

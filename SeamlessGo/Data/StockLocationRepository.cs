using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;

using SeamlessGo.Models;


namespace SeamlessGo.Data
{
    public class StockLocationRepository : IStockLocationRepository
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public StockLocationRepository(IConfiguration configuration, AppDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _context = context;
        }

        public async Task<List<StockLocation>> GetAllAsync()
        {
            return await _context.StockLocation
                .ToListAsync();
        }

    }
}

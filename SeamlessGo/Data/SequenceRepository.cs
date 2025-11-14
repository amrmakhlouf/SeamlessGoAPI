using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class SequenceRepository: ISequenceRepository
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;
        public SequenceRepository(IConfiguration configuration, AppDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _context = context;
        }
        public async Task<IEnumerable<Sequence>> GetAllAsync(int UserID)
        {
            return await _context.Sequences
         .Where(s => s.UserID == UserID)
         .ToListAsync();
        }

    }
}

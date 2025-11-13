using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IStockLocationRepository
    {
        Task<List<StockLocation>> GetAllAsync();
        //Task<StockLocation?> GetByIdAsync(string id);
        //Task<StockLocation> CreateAsync(StockLocation stockLocation);
        //Task<bool> UpdateAsync(string id, StockLocation stockLocation);
        //Task<bool> DeleteAsync(string id);
    }
}

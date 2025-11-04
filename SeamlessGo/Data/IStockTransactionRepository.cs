using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IStockTransactionRepository
    {
        Task<IEnumerable<StockTransaction>> GetAllAsync();
        Task<StockTransaction?> GetByIdAsync(string id);
        Task<StockTransaction?> GetByIdWithLinesAsync(string id);
        Task<StockTransaction?> CreateAsync(StockTransaction stocktransaction, List<StockTransactionLine>? stocktransactionline);
    }
}

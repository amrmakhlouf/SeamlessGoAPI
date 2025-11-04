using SeamlessGo.Models;

namespace SeamlessGo.Data

{
    public interface IStockTransactionLineRepository
    {
        Task<IEnumerable<StockTransactionLine>> GetByTransactionIdAsync(string Transaction);
        Task<StockTransactionLine> CreateAsync(StockTransactionLine stocktransactionLine);
    }
}

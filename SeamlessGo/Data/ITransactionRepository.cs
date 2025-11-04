using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(string id);
        Task<Transaction?> GetByIdWithLinesAsync(string id);
        Task<Transaction?> CreateAsync(Transaction transaction, List<TransactionLine>? orderLines);
    }
}

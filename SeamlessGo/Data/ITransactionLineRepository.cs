using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface ITransactionLineRepository
    {
        Task<IEnumerable<TransactionLine>> GetByTransactionIdAsync(string Transaction);
        Task<TransactionLine> CreateAsync(TransactionLine TransactionLine);
    }
}

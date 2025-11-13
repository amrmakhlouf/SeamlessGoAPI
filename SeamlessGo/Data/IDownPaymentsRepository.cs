using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IDownPaymentsRepository
    {

        Task<DownPayment?> CreateAsync(DownPayment payment, List<DownPaymentAllocations?> DownPaymentAllocations);
        Task<DownPayment?> GetByIdAsync(string id);
        Task<IEnumerable<DownPayment>> GetAllAsync(DateTime? LastModifiedUtc);
    }
}

using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IDownPaymentAllocationsRepository
    {
        Task<IEnumerable<DownPaymentAllocations>> GetByOrderIdAsync(string paymentID);
        Task<DownPaymentAllocations> CreateAsync(DownPaymentAllocations allocation);
    }
}

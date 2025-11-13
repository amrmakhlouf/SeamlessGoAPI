using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IPaymentAllocationsRepository
    {
        Task<IEnumerable<PaymentAllocations>> GetByOrderIdAsync(string paymentID);
        Task<PaymentAllocations> CreateAsync(PaymentAllocations allocation);
    }
}

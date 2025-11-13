using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IPaymentsRepository
    {

        Task<Payments?> CreateAsync(Payments payment, List<PaymentAllocations?> PaymentAllocations);
        Task<Payments?> GetByIdAsync(string id);
        Task<IEnumerable<Payments>> GetAllAsync(DateTime? LastModifiedUtc);


    }
}

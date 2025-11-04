using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IOrderLineRepository
    {
        Task<IEnumerable<OrderLines>> GetByOrderIdAsync(string orderId);
        Task<OrderLines> CreateAsync(OrderLines orderLine);
       // Task<bool> UpdateAsync(int id, OrderLines orderLine);
       // Task<bool> DeleteAsync(int id);
       // Task<bool> DeleteByOrderIdAsync(int orderId);
    }
}

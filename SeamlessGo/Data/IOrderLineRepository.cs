using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IOrderLineRepository
    {
        Task<IEnumerable<OrderLine>> GetByOrderIdAsync(string orderId);
        Task<OrderLine> CreateAsync(OrderLine orderLine);
       // Task<bool> UpdateAsync(int id, OrderLines orderLine);
       // Task<bool> DeleteAsync(int id);
       // Task<bool> DeleteByOrderIdAsync(int orderId);
    }
}

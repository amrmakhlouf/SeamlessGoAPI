using SeamlessGo.Models;

namespace SeamlessGo.Data

{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync(DateTime? LastModifiedUtc);
        Task<Order?> GetByIdAsync(string id);
        Task<Order?> GetByIdWithLinesAsync(string id); // Get order with its lines
        Task<Order> CreateAsync(Order order, List<OrderLine>? orderLines);
  //      Task<bool> UpdateAsync(int id, Orders order);
    //    Task<bool> DeleteAsync(int id);
     //   Task<IEnumerable<Orders>> GetOrdersByCustomerAsync(string customerId);
    }
}

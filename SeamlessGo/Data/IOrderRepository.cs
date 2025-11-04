using SeamlessGo.Models;

namespace SeamlessGo.Data

{
    public interface IOrderRepository
    {
        Task<IEnumerable<Orders>> GetAllAsync();
        Task<Orders?> GetByIdAsync(string id);
        Task<Orders?> GetByIdWithLinesAsync(string id); // Get order with its lines
        Task<Orders> CreateAsync(Orders order, List<OrderLines>? orderLines);
  //      Task<bool> UpdateAsync(int id, Orders order);
    //    Task<bool> DeleteAsync(int id);
     //   Task<IEnumerable<Orders>> GetOrdersByCustomerAsync(string customerId);
    }
}

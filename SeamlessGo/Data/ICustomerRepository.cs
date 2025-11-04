using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(string id);
        Task<Customer> CreateAsync(Customer customer);
        Task<bool> UpdateAsync(string id, Customer customer);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? city);
    }
}

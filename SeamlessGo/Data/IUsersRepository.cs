using SeamlessGo.Models;

namespace SeamlessGo.Data

{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAllAsync(string UserName, string password);
    }
}

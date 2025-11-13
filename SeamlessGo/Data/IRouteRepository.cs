using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IRouteRepository
    {
        Task<SeamlessGo.Models.Route> GetAllAsync(int UserID);
    }
}

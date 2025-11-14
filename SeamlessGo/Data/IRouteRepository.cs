using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IRouteRepository
    {
        Task<SeamlessGo.Models.Route> GetAllAsync(int UserID);
        Task<bool> UpdateStatusAsync(int RouteID, byte Status, DateTime EndDate);
     //   Task<bool> CreateNewVisit(int RouteID);
    }
}

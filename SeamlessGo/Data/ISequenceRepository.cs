
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface ISequenceRepository
    {
        Task <IEnumerable<Sequence>> GetAllAsync(int UserID);
    }
}

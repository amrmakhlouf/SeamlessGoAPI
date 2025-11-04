using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IitemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(string id);
        Task<Item?> GetByIdWithPacksAsync(string id);
        Task<Item> CreateAsync(Item item, List<ItemPack>? itemPacks);
    }
}

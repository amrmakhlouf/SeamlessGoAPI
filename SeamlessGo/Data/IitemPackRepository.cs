using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public interface IitemPackRepository
    {
        Task<IEnumerable<ItemPack>> GetByItemIdAsync(string itemId);
        Task<ItemPack> CreateAsync(ItemPack itemPack);
    
}
}

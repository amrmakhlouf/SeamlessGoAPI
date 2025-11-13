
using Microsoft.AspNetCore.Mvc;
using SeamlessGo.Data;
using SeamlessGo.DTOs;
using SeamlessGo.Models;
namespace SeamlessGo.Controllers
{
 [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IitemRepository _itemRepository;

        public ProductsController(IitemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()
        {
            try
            {
                var items = await _itemRepository.GetAllAsync();
                var itemDTOs = items.Select(MapItemToDTO);
                return Ok(itemDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving items.", error = ex.Message });
            }
        }

        // GET: api/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItem(string id)
        {
            try
            {
                var item = await _itemRepository.GetByIdWithPacksAsync(id);

                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {id} not found." });
                }

                return Ok(MapItemToDTO(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving item.", error = ex.Message });
            }
        }

        // POST: api/items
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItem(CreateItemDTO createItemDTO)
        {
            try
            {
                var item = new Item
                {
                    ItemID = createItemDTO.ItemID,

                    ItemName = createItemDTO.ItemName,
                    ItemCode = createItemDTO.ItemCode,
                    ItemDescription = createItemDTO.ItemDescription,
                    CategoryID = createItemDTO.CategoryID,
                    SupplierID = createItemDTO.SupplierID,
                    BrandID = createItemDTO.BrandID,
                    TaxID = createItemDTO.TaxID,
                    IsActive = createItemDTO.IsActive ?? true,
                    IsVoided = createItemDTO.IsVoided,
                    ItemTypeID = createItemDTO.ItemTypeID,
                    IsStockTracked = createItemDTO.IsStockTracked
                };

                // Convert item packs
                List<ItemPack>? itemPacks = null;
                if (createItemDTO.ItemPacks != null && createItemDTO.ItemPacks.Any())
                {
                    itemPacks = createItemDTO.ItemPacks.Select(DTO => new ItemPack
                    {
                        ItemPackID=DTO.ItemPackID,
                        Equivalency = DTO.Equivalency,
                        IsWeightable = DTO.IsWeightable,
                        UnitID = DTO.UnitID,
                        BarCode = DTO.BarCode,
                        Price = DTO.Price,
                        Cost = DTO.Cost,
                        Enabled = DTO.Enabled,
                        ImagePath = DTO.ImagePath
                    }).ToList();
                }

                // Create item with packs
                var createdItem = await _itemRepository.CreateAsync(item, itemPacks);
                
                // Get complete item with packs
                var itemWithPacks = await _itemRepository.GetByIdWithPacksAsync(createdItem.ItemID);

                return CreatedAtAction(nameof(GetItem), new { id = createdItem.ItemID }, MapItemToDTO(itemWithPacks!));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating item.", error = ex.Message });
            }
        }

        private static ItemDTO MapItemToDTO(Item item)
        {
            return new ItemDTO
            {
                ItemID = item.ItemID,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                ItemDescription = item.ItemDescription,
                CategoryID = item.CategoryID,
                SupplierID = item.SupplierID,
                BrandID = item.BrandID,
                TaxID = item.TaxID,
                IsActive = item.IsActive,
                IsVoided = item.IsVoided,
                CreatedDate = item.CreatedDate,
                ItemTypeID = item.ItemTypeID,
                IsStockTracked = item.IsStockTracked,
                ItemPacks = item.ItemPacks?.Select(pack => new ItemPackDTO
                {
                    ItemPackID = pack.ItemPackID,
                    ItemID = pack.ItemID,
                    Equivalency = pack.Equivalency,
                    IsWeightable = pack.IsWeightable,
                    UnitID = pack.UnitID,
                    BarCode = pack.BarCode,
                    Price = pack.Price,
                    Cost = pack.Cost,
                    Enabled = pack.Enabled,
                    ImagePath = pack.ImagePath
                }).ToList()
            };
        }
    }
}

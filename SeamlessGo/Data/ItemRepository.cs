

using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class ItemRepository : IitemRepository
    {
        private readonly string _connectionString;
        private readonly IitemPackRepository _itemPackRepository;

        public ItemRepository(IConfiguration configuration, IitemPackRepository itemPackRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
            _itemPackRepository = itemPackRepository;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            var items = new List<Item>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Items ORDER BY ItemName", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(MapReaderToItem(reader));
            }

            return items;
        }

        public async Task<Item?> GetByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM dbo.Items WHERE ItemID = @ItemID", connection);

            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToItem(reader);
            }

            return null;
        }

        public async Task<Item?> GetByIdWithPacksAsync(string id)
        {
            var item = await GetByIdAsync(id);

            if (item != null)
            {
                item.ItemPacks = (await _itemPackRepository.GetByItemIdAsync(id)).ToList();
            }

            return item;
        }

        public async Task<Item> CreateAsync(Item item, List<ItemPack>? itemPacks, List<ItemImage>? itemImages)
        {
            const string sql = @"
                INSERT INTO dbo.Items 
                (ItemID, ItemName, ItemCode, ItemDescription, CategoryID, SupplierID, BrandID, TaxID, 
                 IsActive,  CreatedDate, ItemTypeID, IsVoided,IsStockTracked)
                VALUES 
                (@ItemID, @ItemName, @ItemCode, @ItemDescription, @CategoryID, @SupplierID, @BrandID, @TaxID,
                 @IsActive,  @CreatedDate, @ItemTypeID, @Note,@IsStockTracked);";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Use user-provided ItemID (no GUID generation)
                // Use user-provided CreatedDate or default to now
                if (!item.CreatedDate.HasValue)
                {
                    item.CreatedDate = DateTime.Now;
                }

                // Insert item
                using var command = new SqlCommand(sql, connection, transaction);
                AddItemParameters(command, item);
                await command.ExecuteNonQueryAsync();

                // Get the ItemID that user provided
                string itemId = item.ItemID;

                // Insert item packs if provided
                if (itemPacks != null && itemPacks.Any())
                {
                    foreach (var pack in itemPacks)
                    {
                        pack.ItemID = itemId;  // Assign user-provided ItemID
                        await CreateItemPackAsync(connection, transaction, pack);
                    }
                }
                if (itemImages != null && itemImages.Any())
                {
                    foreach (var image in itemImages)
                    {
                        image.ItemID = itemId;  // Assign user-provided ItemID
                        await CreateItemImageAsync(connection, transaction, image);
                    }
                }
                transaction.Commit();
                return item;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task CreateItemImageAsync(SqlConnection connection, SqlTransaction transaction, ItemImage image)
        {
            const string sql = @"
                INSERT INTO dbo.ItemImages 
                (ItemImageID, ItemID, FileName, IsPrimary, SortOrder, LastModifiedUtc)
                VALUES 
                (@ItemImageID, @ItemID, @FileName, @IsPrimary, @SortOrder, @LastModifiedUtc);";

            using var command = new SqlCommand(sql, connection, transaction);

            // Use user-provided ItemImageID (no GUID generation)
            command.Parameters.Add("@ItemImageID", SqlDbType.NVarChar, 50).Value = image.ItemImageID ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = image.ItemID ?? (object)DBNull.Value;
            command.Parameters.Add("@FileName", SqlDbType.NVarChar, 260).Value = image.FileName ?? (object)DBNull.Value;
            command.Parameters.Add("@IsPrimary", SqlDbType.Bit).Value = image.IsPrimary;
            command.Parameters.Add("@SortOrder", SqlDbType.Int).Value = image.SortOrder;
            command.Parameters.Add("@LastModifiedUtc", SqlDbType.DateTime2).Value = (image.LastModifiedUtc == default) ? DateTime.UtcNow : image.LastModifiedUtc;

            await command.ExecuteNonQueryAsync();
        }

        private async Task CreateItemPackAsync(SqlConnection connection, SqlTransaction transaction, ItemPack pack)
        {
            const string sql = @"
                INSERT INTO dbo.ItemPacks 
                (ItemPackID, ItemID, Equivalency, IsWeightable, UnitID, BarCode, Price, Cost, Enabled, ImagePath)
                VALUES 
                (@ItemPackID, @ItemID, @Equivalency, @IsWeightable, @UnitID, @BarCode, @Price, @Cost, @Note, @ImagePath);";

            // Use user-provided ItemPackID (no GUID generation)

            using var command = new SqlCommand(sql, connection, transaction);

            command.Parameters.Add("@ItemPackID", SqlDbType.NVarChar, 50).Value = pack.ItemPackID;
            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = pack.ItemID ?? (object)DBNull.Value;
            command.Parameters.Add("@Equivalency", SqlDbType.Float).Value = pack.Equivalency ?? (object)DBNull.Value;
            command.Parameters.Add("@IsWeightable", SqlDbType.Bit).Value = pack.IsWeightable ?? (object)DBNull.Value;
            command.Parameters.Add("@UnitID", SqlDbType.Int).Value = pack.UnitID ?? (object)DBNull.Value;
            command.Parameters.Add("@BarCode", SqlDbType.NVarChar).Value = pack.BarCode ?? (object)DBNull.Value;
            command.Parameters.Add("@Price", SqlDbType.Decimal).Value = pack.Price ?? (object)DBNull.Value;
            command.Parameters.Add("@Cost", SqlDbType.Decimal).Value = pack.Cost ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.Bit).Value = pack.Enabled ?? (object)DBNull.Value;
            command.Parameters.Add("@ImagePath", SqlDbType.NVarChar).Value = pack.ImagePath ?? (object)DBNull.Value;

            await command.ExecuteNonQueryAsync();
        }

        private static Item MapReaderToItem(SqlDataReader reader)
        {
            return new Item
            {
                ItemID = reader.IsDBNull("ItemID") ? null : reader.GetGuid("ItemID").ToString(),

             //   ItemID = reader.GetString("ItemID"),  // Read as string
                ItemName = reader.IsDBNull("ItemName") ? null : reader.GetString("ItemName"),
                ItemCode = reader.IsDBNull("ItemCode") ? null : reader.GetString("ItemCode"),
                ItemDescription = reader.IsDBNull("ItemDescription") ? null : reader.GetString("ItemDescription"),
                CategoryID = reader.IsDBNull("CategoryID") ? null : reader.GetInt32("CategoryID"),
                SupplierID = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                BrandID = reader.IsDBNull("BrandID") ? null : reader.GetInt32("BrandID"),
                TaxID = reader.IsDBNull("TaxID") ? null : reader.GetInt32("TaxID"),
                IsActive = reader.IsDBNull("IsActive") ? null : reader.GetBoolean("IsActive"),
                IsVoided = reader.IsDBNull("IsVoided") ? null : reader.GetBoolean("IsVoided"),
                CreatedDate = reader.IsDBNull("CreatedDate") ? null : reader.GetDateTime("CreatedDate"),
                ItemTypeID = reader.IsDBNull("ItemTypeID") ? null : reader.GetInt32("ItemTypeID"),
                IsStockTracked = reader.IsDBNull("IsStockTracked") ? null : reader.GetBoolean("IsStockTracked")
            };
        }

        private static void AddItemParameters(SqlCommand command, Item item)
        {
            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = item.ItemID;  
            command.Parameters.Add("@ItemName", SqlDbType.NVarChar).Value = item.ItemName ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemCode", SqlDbType.NVarChar).Value = item.ItemCode ?? (object)DBNull.Value;
            command.Parameters.Add("@ItemDescription", SqlDbType.NVarChar).Value = item.ItemDescription ?? (object)DBNull.Value;
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = item.CategoryID ?? (object)DBNull.Value;
            command.Parameters.Add("@SupplierID", SqlDbType.Int).Value = item.SupplierID ?? (object)DBNull.Value;
            command.Parameters.Add("@BrandID", SqlDbType.Int).Value = item.BrandID ?? (object)DBNull.Value;
            command.Parameters.Add("@TaxID", SqlDbType.Int).Value = item.TaxID ?? (object)DBNull.Value;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.NVarChar).Value = item.IsVoided ?? (object)DBNull.Value;
            command.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = item.CreatedDate ?? DateTime.Now;
            command.Parameters.Add("@ItemTypeID", SqlDbType.Int).Value = item.ItemTypeID ?? (object)DBNull.Value;
            command.Parameters.Add("@IsStockTracked", SqlDbType.Bit).Value = item.IsStockTracked ?? (object)DBNull.Value;
        }
    }
}

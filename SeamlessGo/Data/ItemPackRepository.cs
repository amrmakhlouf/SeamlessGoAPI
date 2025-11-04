using System.Data;
using System.Data.SqlClient;
using SeamlessGo.Models;

namespace SeamlessGo.Data
{
    public class ItemPackRepository : IitemPackRepository
    {
        private readonly string _connectionString;

        public ItemPackRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string not found");
        }

        public async Task<IEnumerable<ItemPack>> GetByItemIdAsync(string itemId)
        {
            var itemPacks = new List<ItemPack>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT * FROM dbo.ItemPacks WHERE ItemID = @ItemID",
                connection);

            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = itemId;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                itemPacks.Add(MapReaderToItemPack(reader));
            }

            return itemPacks;
        }

        public async Task<ItemPack> CreateAsync(ItemPack itemPack)
        {
            const string sql = @"
                INSERT INTO dbo.ItemPacks 
                (ItemPackID, ItemID, Equivalency, IsWeightable, UnitID, BarCode, Price, Cost, Enabled, ImagePath)
                VALUES 
                (@ItemPackID, @ItemID, @Equivalency, @IsWeightable, @UnitID, @BarCode, @Price, @Cost, @Note, @ImagePath);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            // Use user-provided ItemPackID

            command.Parameters.Add("@ItemPackID", SqlDbType.NVarChar, 50).Value = itemPack.ItemPackID;
            command.Parameters.Add("@ItemID", SqlDbType.NVarChar, 50).Value = itemPack.ItemID ?? (object)DBNull.Value;
            command.Parameters.Add("@Equivalency", SqlDbType.Decimal).Value = itemPack.Equivalency ?? (object)DBNull.Value;
            command.Parameters.Add("@IsWeightable", SqlDbType.Bit).Value = itemPack.IsWeightable ?? (object)DBNull.Value;
            command.Parameters.Add("@UnitID", SqlDbType.Int).Value = itemPack.UnitID ?? (object)DBNull.Value;
            command.Parameters.Add("@BarCode", SqlDbType.NVarChar).Value = itemPack.BarCode ?? (object)DBNull.Value;
            command.Parameters.Add("@Price", SqlDbType.Decimal).Value = itemPack.Price ?? (object)DBNull.Value;
            command.Parameters.Add("@Cost", SqlDbType.Decimal).Value = itemPack.Cost ?? (object)DBNull.Value;
            command.Parameters.Add("@Note", SqlDbType.Bit).Value = itemPack.Enabled ?? (object)DBNull.Value;
            command.Parameters.Add("@ImagePath", SqlDbType.NVarChar).Value = itemPack.ImagePath ?? (object)DBNull.Value;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return itemPack;
        }

        private static ItemPack MapReaderToItemPack(SqlDataReader reader)
        {
            return new ItemPack
            {
                ItemPackID = reader.IsDBNull("ItemPackID") ? null : reader.GetGuid("ItemPackID").ToString(),
                ItemID = reader.IsDBNull("ItemID") ? null : reader.GetGuid("ItemID").ToString(),

                // ItemID = reader.IsDBNull("ItemID") ? null : reader.GetString("ItemID"),  // Read as string
                Equivalency = reader.IsDBNull("Equivalency") ? null : reader.GetFloat("Equivalency"),
                IsWeightable = reader.IsDBNull("IsWeightable") ? null : reader.GetBoolean("IsWeightable"),
                UnitID = reader.IsDBNull("UnitID") ? null : reader.GetInt32("UnitID"),
                BarCode = reader.IsDBNull("BarCode") ? null : reader.GetString("BarCode"),
                Price = reader.IsDBNull("Price") ? null : reader.GetDecimal("Price"),
                Cost = reader.IsDBNull("Cost") ? null : reader.GetDecimal("Cost"),
                Enabled = reader.IsDBNull("Enabled") ? null : reader.GetBoolean("Enabled"),
                ImagePath = reader.IsDBNull("ImagePath") ? null : reader.GetString("ImagePath")
            };
        }
    }
}

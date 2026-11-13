using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Manages the shopping list, including automatically adding low-stock types and clearing restocked ones.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class ShoppingListService(StockWiseDbContext db)
    {
        /// <summary>
        /// Returns every shopping list entry, ordered by name.
        /// </summary>
        public async Task<List<ShoppingListItem>> GetAllAsync()
        {
            List<ShoppingListItem> items = await db.ShoppingListItems.Include(x => x.Type).ToListAsync();
            return [.. items.OrderBy(x => x.DisplayName)];
        }

        /// <summary>
        /// Adds an entry by name, linking it to the item type of the same name if one exists, otherwise as free text.
        /// </summary>
        /// <param name="name">The name to add.</param>
        public async Task AddAsync(string name)
        {
            string trimmed = name.Trim();
            if (trimmed.Length == 0)
            {
                return;
            }

            string lowered = trimmed.ToLower();
            ItemType? type = await db.Types.FirstOrDefaultAsync(x => x.Name.ToLower() == lowered);
            if (type is not null)
            {
                await AddTypeAsync(type.TypeId, isAutoAdded: false);
                return;
            }

            bool exists = await db.ShoppingListItems.AnyAsync(x => x.TypeId == null && x.Name!.ToLower() == lowered);
            if (!exists)
            {
                db.ShoppingListItems.Add(new ShoppingListItem { Name = trimmed, AddedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes an entry from the list.
        /// </summary>
        /// <param name="shoppingListItemId">The ID of the entry to remove.</param>
        public async Task RemoveAsync(int shoppingListItemId)
        {
            ShoppingListItem? item = await db.ShoppingListItems.FindAsync(shoppingListItemId);
            if (item is not null)
            {
                db.ShoppingListItems.Remove(item);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds the item's type to the list if its unopened stock has fallen to the type's minimum quantity.
        /// </summary>
        /// <param name="itemId">The ID of the item whose stock went down.</param>
        public async Task CheckLowStockForItemAsync(int itemId)
        {
            int? typeId = await db.Items.Where(x => x.ItemId == itemId).Select(x => x.TypeId).FirstOrDefaultAsync();
            if (typeId is not null)
            {
                await CheckLowStockForTypeAsync(typeId.Value);
            }
        }

        /// <summary>
        /// Adds a type to the list if its unopened stock, across all its items, is at or below its minimum quantity.
        /// </summary>
        /// <param name="typeId">The ID of the type to check.</param>
        public async Task CheckLowStockForTypeAsync(int typeId)
        {
            int? minQuantity = await db.Types.Where(x => x.TypeId == typeId).Select(x => x.MinQuantity).FirstOrDefaultAsync();
            if (minQuantity is null)
            {
                return;
            }

            int unopened = await db.Stock.Where(x => x.Item!.TypeId == typeId && x.OpenedAt == null).SumAsync(x => x.Quantity);
            if (unopened <= minQuantity)
            {
                await AddTypeAsync(typeId, isAutoAdded: true);
            }
        }

        /// <summary>
        /// Removes the item's type from the list, since it's just been restocked.
        /// </summary>
        /// <param name="itemId">The ID of the item that was restocked.</param>
        public async Task ClearRestockedItemAsync(int itemId)
        {
            int? typeId = await db.Items.Where(x => x.ItemId == itemId).Select(x => x.TypeId).FirstOrDefaultAsync();
            if (typeId is null)
            {
                return;
            }

            List<ShoppingListItem> entries = await db.ShoppingListItems.Where(x => x.TypeId == typeId).ToListAsync();
            if (entries.Count > 0)
            {
                db.ShoppingListItems.RemoveRange(entries);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds a type to the list, unless it's already on it.
        /// </summary>
        /// <param name="typeId">The ID of the type to add.</param>
        /// <param name="isAutoAdded">Whether the type is being added automatically for low stock.</param>
        private async Task AddTypeAsync(int typeId, bool isAutoAdded)
        {
            bool exists = await db.ShoppingListItems.AnyAsync(x => x.TypeId == typeId);
            if (!exists)
            {
                db.ShoppingListItems.Add(new ShoppingListItem { TypeId = typeId, IsAutoAdded = isAutoAdded, AddedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
        }
    }
}
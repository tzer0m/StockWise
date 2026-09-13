using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Provides item lookup, creation, and editing logic shared across the pages that scan, add, edit, or bulk-add items.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class ItemService(StockWiseDbContext db, HistoryService historyService)
    {
        /// <summary>
        /// Finds an item by its barcode, including its storage category allowances.
        /// </summary>
        /// <param name="barcode">The barcode to look up.</param>
        public async Task<Item?> FindByBarcodeAsync(string barcode)
        {
            return await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.Barcode == barcode);
        }

        /// <summary>
        /// Finds an item by its ID, including its storage category allowances.
        /// </summary>
        /// <param name="itemId">The ID of the item to find.</param>
        public async Task<Item?> FindByIdAsync(int itemId)
        {
            return await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.ItemId == itemId);
        }

        /// <summary>
        /// Returns the locations allowed for an item while unopened, ordered by category then name.
        /// </summary>
        /// <param name="item">The item to find allowed locations for.</param>
        public async Task<List<Location>> GetAllowedLocationsAsync(Item item)
        {
            List<int> allowedCategoryIds = item.ItemStorageCategories.Where(x => x.AllowedWhenUnopened).Select(x => x.CategoryId).ToList();
            return await db.Locations.Include(x => x.Category).Where(x => allowedCategoryIds.Contains(x.CategoryId)).OrderBy(x => x.Category!.Name).ThenBy(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Returns a category allowance row for every storage category, marking any the given item already has, or none if no item is given.
        /// </summary>
        /// <param name="item">The item whose existing allowances should be marked, if any.</param>
        public async Task<List<CategoryAllowance>> GetCategoryAllowancesAsync(Item? item = null)
        {
            List<StorageCategory> categories = await db.StorageCategories.OrderBy(x => x.Name).ToListAsync();
            return [.. categories.Select(x => new CategoryAllowance { CategoryId = x.CategoryId, CategoryName = x.Name, AllowedWhenUnopened = item is not null && item.ItemStorageCategories.Any(y => y.CategoryId == x.CategoryId && y.AllowedWhenUnopened), AllowedWhenOpened = item is not null && item.ItemStorageCategories.Any(y => y.CategoryId == x.CategoryId && y.AllowedWhenOpened) })];
        }

        /// <summary>
        /// Returns every item type, ordered by name.
        /// </summary>
        public async Task<List<ItemType>> GetTypesAsync()
        {
            return await db.Types.OrderBy(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Creates a new item along with its storage category allowances.
        /// </summary>
        /// <param name="input">The item's fields, as submitted on the form.</param>
        public async Task<Item> CreateAsync(ItemFormInput input)
        {
            Item item = new() { Barcode = input.Barcode.Trim(), Name = input.Name.Trim(), Brand = input.Brand, TypeId = input.TypeId, ImageUrl = input.ImageUrl, IsOpenable = input.IsOpenable, ExpiryAfterOpeningDays = input.IsOpenable ? input.ExpiryAfterOpeningDays : null, CreatedAt = DateTime.UtcNow };
            ApplyCategoryAllowances(item, input.CategoryAllowances, input.IsOpenable);
            db.Items.Add(item);
            await db.SaveChangesAsync();
            await historyService.LogItemAddedAsync(item.Name);
            return item;
        }

        /// <summary>
        /// Updates an existing item's details and replaces its storage category allowances.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <param name="input">The item's fields, as submitted on the form.</param>
        public async Task UpdateAsync(Item item, ItemFormInput input)
        {
            item.Barcode = input.Barcode.Trim();
            item.Name = input.Name.Trim();
            item.Brand = input.Brand;
            item.TypeId = input.TypeId;
            item.ImageUrl = input.ImageUrl;
            item.IsOpenable = input.IsOpenable;
            item.ExpiryAfterOpeningDays = input.IsOpenable ? input.ExpiryAfterOpeningDays : null;
            item.ItemStorageCategories.Clear();
            ApplyCategoryAllowances(item, input.CategoryAllowances, input.IsOpenable);
            await db.SaveChangesAsync();
            await historyService.LogItemUpdatedAsync(item.Name);
        }

        /// <summary>
        /// Returns every item's barcode and display name, for client-side matching while scanning a batch.
        /// </summary>
        public async Task<List<BarcodeNameLookup>> GetBarcodeNameLookupAsync()
        {
            return await db.Items.Select(x => new BarcodeNameLookup { Barcode = x.Barcode, Name = x.Name }).ToListAsync();
        }

        /// <summary>
        /// Adds a storage category allowance to an item for every allowance that's been selected.
        /// </summary>
        /// <param name="item">The item to add allowances to.</param>
        /// <param name="categoryAllowances">The category allowances chosen for the item.</param>
        /// <param name="isOpenable">Whether a unit of the item can be opened.</param>
        private static void ApplyCategoryAllowances(Item item, List<CategoryAllowance> categoryAllowances, bool isOpenable)
        {
            foreach (CategoryAllowance allowance in categoryAllowances.Where(x => x.AllowedWhenUnopened || (isOpenable && x.AllowedWhenOpened)))
            {
                item.ItemStorageCategories.Add(new ItemStorageCategory { ItemId = item.ItemId, CategoryId = allowance.CategoryId, AllowedWhenUnopened = allowance.AllowedWhenUnopened, AllowedWhenOpened = isOpenable && allowance.AllowedWhenOpened });
            }
        }
    }
}
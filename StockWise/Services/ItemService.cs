using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Provides item lookup, creation, and editing logic shared across the pages that scan, add, edit, or bulk-add items.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class ItemService(StockWiseDbContext db)
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
        /// Creates a new item along with its storage category allowances.
        /// </summary>
        /// <param name="barcode">The item's barcode.</param>
        /// <param name="name">The item's display name.</param>
        /// <param name="imageUrl">A URL to an image of the item, if any.</param>
        /// <param name="isOpenable">Whether a unit of this item can be opened.</param>
        /// <param name="expiryAfterOpeningDays">How many days after opening this item expires, if it's openable.</param>
        /// <param name="categoryAllowances">The category allowances chosen for the item.</param>
        public async Task<Item> CreateAsync(string barcode, string name, string? imageUrl, bool isOpenable, int? expiryAfterOpeningDays, List<CategoryAllowance> categoryAllowances)
        {
            Item item = new() { Barcode = barcode.Trim(), Name = name.Trim(), ImageUrl = imageUrl, IsOpenable = isOpenable, ExpiryAfterOpeningDays = isOpenable ? expiryAfterOpeningDays : null, CreatedAt = DateTime.UtcNow };
            ApplyCategoryAllowances(item, categoryAllowances, isOpenable);
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return item;
        }

        /// <summary>
        /// Updates an existing item's details and replaces its storage category allowances.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <param name="barcode">The item's barcode.</param>
        /// <param name="name">The item's display name.</param>
        /// <param name="imageUrl">A URL to an image of the item, if any.</param>
        /// <param name="isOpenable">Whether a unit of this item can be opened.</param>
        /// <param name="expiryAfterOpeningDays">How many days after opening this item expires, if it's openable.</param>
        /// <param name="categoryAllowances">The category allowances chosen for the item.</param>
        public async Task UpdateAsync(Item item, string barcode, string name, string? imageUrl, bool isOpenable, int? expiryAfterOpeningDays, List<CategoryAllowance> categoryAllowances)
        {
            item.Barcode = barcode.Trim();
            item.Name = name.Trim();
            item.ImageUrl = imageUrl;
            item.IsOpenable = isOpenable;
            item.ExpiryAfterOpeningDays = isOpenable ? expiryAfterOpeningDays : null;
            item.ItemStorageCategories.Clear();
            ApplyCategoryAllowances(item, categoryAllowances, isOpenable);
            await db.SaveChangesAsync();
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
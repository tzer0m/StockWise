using Microsoft.EntityFrameworkCore;
using StockWise.Data;

namespace StockWise.Services
{
    /// <summary>
    /// Provides stock addition logic shared across the pages that add stock for an item.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class StockService(StockWiseDbContext db)
    {
        /// <summary>
        /// Adds stock for an item at a location, merging into a matching existing unopened row if one exists.
        /// </summary>
        /// <param name="itemId">The ID of the item to add stock for.</param>
        /// <param name="locationId">The location to add stock at.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <param name="expiry">The expiry date for the added stock, if any.</param>
        public async Task AddOrMergeAsync(int itemId, int locationId, int quantity, DateOnly? expiry)
        {
            Models.Stock? existing = await db.Stock.FirstOrDefaultAsync(x => x.ItemId == itemId && x.LocationId == locationId && x.Expiry == expiry && x.OpenedAt == null);
            if (existing is not null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                db.Stock.Add(new Models.Stock { ItemId = itemId, LocationId = locationId, Quantity = quantity, Expiry = expiry, AddedAt = DateTime.UtcNow });
            }

            await db.SaveChangesAsync();
        }
    }
}
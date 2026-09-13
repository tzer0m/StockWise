using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Provides stock addition logic shared across the pages that add stock for an item.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class StockService(StockWiseDbContext db, HistoryService historyService)
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
            Stock? existing = await db.Stock.FirstOrDefaultAsync(x => x.ItemId == itemId && x.LocationId == locationId && x.Expiry == expiry && x.OpenedAt == null);
            if (existing is not null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                db.Stock.Add(new Stock { ItemId = itemId, LocationId = locationId, Quantity = quantity, Expiry = expiry, AddedAt = DateTime.UtcNow });
            }

            await db.SaveChangesAsync();

            Item? item = await db.Items.FindAsync(itemId);
            Location? location = await db.Locations.FindAsync(locationId);
            if (item is not null && location is not null)
            {
                await historyService.LogStockAddedAsync(item.Name, quantity, location.Name);
            }
        }
    }
}
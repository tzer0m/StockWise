using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Stock
{
    /// <summary>
    /// Page model for moving some or all of a stock row to a different location.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class MoveModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// The stock row being moved.
        /// </summary>
        public StockWise.Models.Stock? Stock { get; set; }

        /// <summary>
        /// The heading to show for the item being moved, formatted as "Brand Name", omitting the brand when it's unknown.
        /// </summary>
        public string ItemTitle => Stock?.Item is null ? string.Empty : string.IsNullOrWhiteSpace(Stock.Item.Brand) ? Stock.Item.Name : $"{Stock.Item.Brand} {Stock.Item.Name}";

        /// <summary>
        /// The locations this stock can be moved to in its current opened/unopened state.
        /// </summary>
        public List<Location> AllowedLocations { get; set; } = [];

        /// <summary>
        /// The location to move the stock to.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Select a location.")]
        public int LocationId { get; set; }

        /// <summary>
        /// The number of units to move.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a quantity of at least 1.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Loads the stock row to move and its allowed destination locations.
        /// </summary>
        /// <param name="id">The ID of the stock row to move.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!await LoadStockAsync(id))
            {
                return NotFound();
            }

            Quantity = Stock!.Quantity;
            return Page();
        }

        /// <summary>
        /// Moves the chosen quantity to the new location, merging into a matching unopened row if one exists.
        /// </summary>
        /// <param name="id">The ID of the stock row to move.</param>
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!await LoadStockAsync(id))
            {
                return NotFound();
            }

            if (Quantity > Stock!.Quantity)
            {
                ModelState.AddModelError(nameof(Quantity), $"Only {Stock.Quantity} available to move.");
            }

            if (!AllowedLocations.Any(x => x.LocationId == LocationId))
            {
                ModelState.AddModelError(nameof(LocationId), "Select a valid location.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string fromLocationName = Stock.Location?.Name ?? string.Empty;
            string toLocationName = AllowedLocations.First(x => x.LocationId == LocationId).Name;
            StockWise.Models.Stock? existing = Stock.OpenedAt is null ? await db.Stock.FirstOrDefaultAsync(x => x.ItemId == Stock.ItemId && x.LocationId == LocationId && x.Expiry == Stock.Expiry && x.OpenedAt == null) : null;
            if (existing is not null)
            {
                existing.Quantity += Quantity;
                Stock.Quantity -= Quantity;
                if (Stock.Quantity <= 0)
                {
                    db.Stock.Remove(Stock);
                }
            }
            else if (Quantity == Stock.Quantity)
            {
                Stock.LocationId = LocationId;
            }
            else
            {
                Stock.Quantity -= Quantity;
                db.Stock.Add(new StockWise.Models.Stock { ItemId = Stock.ItemId, LocationId = LocationId, Quantity = Quantity, Expiry = Stock.Expiry, AddedAt = Stock.AddedAt, OpenedAt = Stock.OpenedAt });
            }

            await db.SaveChangesAsync();
            await historyService.LogStockMovedAsync(ItemTitle, Quantity, fromLocationName, toLocationName);
            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Loads the stock row and the locations it can be moved to.
        /// </summary>
        /// <param name="id">The ID of the stock row to load.</param>
        private async Task<bool> LoadStockAsync(int id)
        {
            Stock = await db.Stock.Include(x => x.Location).ThenInclude(x => x!.Category).Include(x => x.Item).ThenInclude(x => x!.ItemStorageCategories).FirstOrDefaultAsync(x => x.StockId == id);
            if (Stock is null)
            {
                return false;
            }

            bool isOpened = Stock.OpenedAt is not null;
            List<int> allowedCategoryIds = [.. Stock.Item!.ItemStorageCategories.Where(x => isOpened ? x.AllowedWhenOpened : x.AllowedWhenUnopened).Select(x => x.CategoryId)];
            int currentLocationId = Stock.LocationId;
            AllowedLocations = await db.Locations.Include(x => x.Category).Where(x => allowedCategoryIds.Contains(x.CategoryId) && x.LocationId != currentLocationId).OrderBy(x => x.Category!.Name).ThenBy(x => x.Name).ToListAsync();
            return true;
        }
    }
}
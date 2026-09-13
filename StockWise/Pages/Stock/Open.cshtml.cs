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
    /// Page model for opening a unit of stock, moving it to a new location.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class OpenModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// The stock row being opened.
        /// </summary>
        public StockWise.Models.Stock? Stock { get; set; }

        /// <summary>
        /// The heading to show for the item being opened, formatted as "Brand Name", omitting the brand when it's unknown.
        /// </summary>
        public string ItemTitle => Stock?.Item is null
            ? string.Empty
            : string.IsNullOrWhiteSpace(Stock.Item.Brand)
                ? Stock.Item.Name
                : $"{Stock.Item.Brand} {Stock.Item.Name}";

        /// <summary>
        /// The locations allowed for this item once opened.
        /// </summary>
        public List<Location> AllowedLocations { get; set; } = [];

        /// <summary>
        /// The new location to move the opened unit to.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Select a location.")]
        public int LocationId { get; set; }

        /// <summary>
        /// The new expiry date for the opened unit, if any.
        /// </summary>
        [BindProperty]
        public DateOnly? Expiry { get; set; }

        /// <summary>
        /// Loads the stock row to open and its allowed opened locations.
        /// </summary>
        /// <param name="id">The ID of the stock row to open.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!await LoadStockAsync(id))
            {
                return NotFound();
            }

            LocationId = Stock!.LocationId;
            if (Stock.Item?.ExpiryAfterOpeningDays is int expiryAfterOpeningDays)
            {
                Expiry = DateOnly.FromDateTime(DateTime.Today).AddDays(expiryAfterOpeningDays);
            }

            return Page();
        }

        /// <summary>
        /// Splits off one opened unit into a new stock row at the chosen location.
        /// </summary>
        /// <param name="id">The ID of the stock row to open.</param>
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!await LoadStockAsync(id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Stock!.Quantity--;
            if (Stock.Quantity <= 0)
            {
                db.Stock.Remove(Stock);
            }

            db.Stock.Add(new StockWise.Models.Stock { ItemId = Stock.ItemId, LocationId = LocationId, Quantity = 1, Expiry = Expiry, AddedAt = DateTime.UtcNow, OpenedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            Location? newLocation = await db.Locations.FindAsync(LocationId);
            await historyService.LogStockOpenedAsync(ItemTitle, newLocation?.Name ?? string.Empty);
            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Loads the stock row and its allowed opened locations.
        /// </summary>
        /// <param name="id">The ID of the stock row to load.</param>
        private async Task<bool> LoadStockAsync(int id)
        {
            Stock = await db.Stock.Include(x => x.Item).ThenInclude(x => x!.ItemStorageCategories).FirstOrDefaultAsync(x => x.StockId == id);
            if (Stock is null)
            {
                return false;
            }

            List<int> allowedCategoryIds = [.. Stock.Item!.ItemStorageCategories.Where(x => x.AllowedWhenOpened).Select(x => x.CategoryId)];
            AllowedLocations = await db.Locations.Include(x => x.Category).Where(x => allowedCategoryIds.Contains(x.CategoryId)).OrderBy(x => x.Category!.Name).ThenBy(x => x.Name).ToListAsync();
            return true;
        }
    }
}
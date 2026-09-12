using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Stock
{
    /// <summary>
    /// Page model for scanning an item's barcode and adding stock for it.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class AddModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// The scanned barcode.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The item matching the scanned barcode, if found.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// The locations allowed for this item while unopened.
        /// </summary>
        public List<SelectListItem> LocationOptions { get; set; } = [];

        /// <summary>
        /// The location to add stock at.
        /// </summary>
        [BindProperty]
        public int LocationId { get; set; }

        /// <summary>
        /// The quantity to add.
        /// </summary>
        [BindProperty]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The expiry date for the added stock, if any.
        /// </summary>
        [BindProperty]
        public DateOnly? Expiry { get; set; }

        /// <summary>
        /// A message to show after adding stock.
        /// </summary>
        [TempData]
        public string? Message { get; set; }

        /// <summary>
        /// Looks up the item for the scanned barcode, if any.
        /// </summary>
        public async Task OnGetAsync()
        {
            await LoadItemAsync();
        }

        /// <summary>
        /// Adds stock for the scanned item, merging with a matching existing row if one exists.
        /// </summary>
        /// <param name="itemId">The ID of the item to add stock for.</param>
        public async Task<IActionResult> OnPostAddStockAsync(int itemId)
        {
            StockWise.Models.Stock? existing = await db.Stock.FirstOrDefaultAsync(x => x.ItemId == itemId && x.LocationId == LocationId && x.Expiry == Expiry && x.OpenedAt == null);
            if (existing is not null)
            {
                existing.Quantity += Quantity;
            }
            else
            {
                db.Stock.Add(new StockWise.Models.Stock { ItemId = itemId, LocationId = LocationId, Quantity = Quantity, Expiry = Expiry, AddedAt = DateTime.UtcNow });
            }

            await db.SaveChangesAsync();
            Message = "Stock added.";
            return RedirectToPage();
        }

        /// <summary>
        /// Loads the scanned item and its allowed unopened locations.
        /// </summary>
        private async Task LoadItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                return;
            }

            Item = await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.Barcode == Barcode);
            if (Item is null)
            {
                return;
            }

            List<int> allowedCategoryIds = Item.ItemStorageCategories.Where(x => x.AllowedWhenUnopened).Select(x => x.CategoryId).ToList();
            List<Location> locations = await db.Locations.Include(x => x.Category).Where(x => allowedCategoryIds.Contains(x.CategoryId)).OrderBy(x => x.Category!.Name).ThenBy(x => x.Name).ToListAsync();
            LocationOptions = locations.Select(x => new SelectListItem($"{x.Category!.Name} - {x.Name}", x.LocationId.ToString())).ToList();
        }
    }
}
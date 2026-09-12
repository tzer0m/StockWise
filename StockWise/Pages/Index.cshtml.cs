using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages
{
    /// <summary>
    /// Page model for the home page: a barcode scan hub, plus a single flat table of all current stock.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All current stock rows, with their item, location, and category loaded.
        /// </summary>
        public List<StockWise.Models.Stock> AllStock { get; set; } = [];

        /// <summary>
        /// The scanned barcode.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The item matching the scanned barcode, if found.
        /// </summary>
        public Item? ScannedItem { get; set; }

        /// <summary>
        /// The scanned item's current stock rows, with their locations loaded.
        /// </summary>
        public List<StockWise.Models.Stock> ScannedItemStock { get; set; } = [];

        /// <summary>
        /// Loads all current stock, plus the scanned item if a barcode was given.
        /// </summary>
        public async Task OnGetAsync()
        {
            AllStock = await db.Stock.Include(x => x.Item).Include(x => x.Location).ThenInclude(x => x!.Category).OrderBy(x => x.Location!.Category!.Name).ThenBy(x => x.Location!.Name).ThenBy(x => x.Item!.Name).ToListAsync();
            await LoadScannedItemAsync();
        }

        /// <summary>
        /// Checks out one unit of stock from a row, deleting it if it reaches zero.
        /// </summary>
        /// <param name="stockId">The ID of the stock row to check out from.</param>
        public async Task<IActionResult> OnPostCheckoutAsync(int stockId)
        {
            StockWise.Models.Stock? stock = await db.Stock.FindAsync(stockId);
            if (stock is not null)
            {
                stock.Quantity--;
                if (stock.Quantity <= 0)
                {
                    db.Stock.Remove(stock);
                }

                await db.SaveChangesAsync();
            }

            return RedirectToPage(new { Barcode });
        }

        /// <summary>
        /// Finishes an opened stock row, removing it entirely.
        /// </summary>
        /// <param name="stockId">The ID of the stock row to finish.</param>
        public async Task<IActionResult> OnPostFinishAsync(int stockId)
        {
            StockWise.Models.Stock? stock = await db.Stock.FindAsync(stockId);
            if (stock is not null)
            {
                db.Stock.Remove(stock);
                await db.SaveChangesAsync();
            }

            return RedirectToPage(new { Barcode });
        }

        /// <summary>
        /// Looks up the item for the scanned barcode and its current stock rows, if any.
        /// </summary>
        private async Task LoadScannedItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                return;
            }

            ScannedItem = await db.Items.FirstOrDefaultAsync(x => x.Barcode == Barcode);
            if (ScannedItem is null)
            {
                return;
            }

            ScannedItemStock = await db.Stock.Include(x => x.Location).Where(x => x.ItemId == ScannedItem.ItemId).OrderBy(x => x.Location!.Name).ToListAsync();
        }
    }
}
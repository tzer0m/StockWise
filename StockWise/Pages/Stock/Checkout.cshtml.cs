using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Stock
{
    /// <summary>
    /// Page model for scanning an item's barcode and checking out a unit of stock.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class CheckoutModel(StockWiseDbContext db) : PageModel
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
        /// The item's current stock rows, with their locations loaded.
        /// </summary>
        public List<StockWise.Models.Stock> StockRows { get; set; } = [];

        /// <summary>
        /// A message to show after checking out stock.
        /// </summary>
        [TempData]
        public string? Message { get; set; }

        /// <summary>
        /// Looks up the item for the scanned barcode and its current stock, if any.
        /// </summary>
        public async Task OnGetAsync()
        {
            await LoadItemAsync();
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
                Message = "Checked out.";
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Loads the scanned item and its current stock rows.
        /// </summary>
        private async Task LoadItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                return;
            }

            Item = await db.Items.FirstOrDefaultAsync(x => x.Barcode == Barcode);
            if (Item is null)
            {
                return;
            }

            StockRows = await db.Stock.Include(x => x.Location).Where(x => x.ItemId == Item.ItemId).OrderBy(x => x.Location!.Name).ToListAsync();
        }
    }
}
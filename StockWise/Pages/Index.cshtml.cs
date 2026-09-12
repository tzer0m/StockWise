using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages
{
    /// <summary>
    /// Page model for the home page: a barcode scan hub, plus a single sortable table of all current stock.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All current stock rows, sorted, with their item, location, and category loaded.
        /// </summary>
        public List<StockWise.Models.Stock> AllStock { get; set; } = [];

        /// <summary>
        /// The scanned barcode.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The column to sort the stock table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "expiry";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "asc";

        /// <summary>
        /// The item matching the scanned barcode, if found.
        /// </summary>
        public Item? ScannedItem { get; set; }

        /// <summary>
        /// The scanned item's current stock rows, with their locations loaded.
        /// </summary>
        public List<StockWise.Models.Stock> ScannedItemStock { get; set; } = [];

        /// <summary>
        /// Loads the sorted stock, plus the scanned item if a barcode was given.
        /// </summary>
        public async Task OnGetAsync()
        {
            IQueryable<StockWise.Models.Stock> query = db.Stock.Include(x => x.Item).Include(x => x.Location).ThenInclude(x => x!.Category);
            AllStock = await ApplySort(query, Sort, Direction == "desc").ToListAsync();
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

            return RedirectToPage(new { Barcode, Sort, Direction });
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

            return RedirectToPage(new { Barcode, Sort, Direction });
        }

        /// <summary>
        /// Applies the requested sort, always pushing stock with no expiry date to the end regardless of direction.
        /// </summary>
        /// <param name="query">The stock query to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static IOrderedQueryable<StockWise.Models.Stock> ApplySort(IQueryable<StockWise.Models.Stock> query, string sort, bool descending)
        {
            return sort switch
            {
                "item" => descending ? query.OrderByDescending(x => x.Item!.Name) : query.OrderBy(x => x.Item!.Name),
                "location" => descending ? query.OrderByDescending(x => x.Location!.Name) : query.OrderBy(x => x.Location!.Name),
                "quantity" => descending ? query.OrderByDescending(x => x.Quantity) : query.OrderBy(x => x.Quantity),
                "opened" => descending ? query.OrderByDescending(x => x.OpenedAt) : query.OrderBy(x => x.OpenedAt),
                _ => descending ? query.OrderBy(x => x.Expiry == null).ThenByDescending(x => x.Expiry) : query.OrderBy(x => x.Expiry == null).ThenBy(x => x.Expiry),
            };
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
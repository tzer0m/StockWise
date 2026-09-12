using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages
{
    /// <summary>
    /// Page model for the home page, showing all current stock grouped by category.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All storage categories, with their locations and stock loaded.
        /// </summary>
        public List<StorageCategory> Categories { get; set; } = [];

        /// <summary>
        /// Loads all categories, locations, and current stock.
        /// </summary>
        public async Task OnGetAsync()
        {
            Categories = await db.StorageCategories.Include(x => x.Locations).ThenInclude(x => x.Stock).ThenInclude(x => x.Item).OrderBy(x => x.Name).ToListAsync();
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

            return RedirectToPage();
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

            return RedirectToPage();
        }
    }
}
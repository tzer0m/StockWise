using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Manage.Items
{
    /// <summary>
    /// Page model for listing and deleting trackable items.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All items, ordered by name.
        /// </summary>
        public List<Item> Items { get; set; } = [];

        /// <summary>
        /// An error message to show, if the last action failed.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loads all items.
        /// </summary>
        public async Task OnGetAsync()
        {
            Items = await db.Items.OrderBy(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Deletes an item, unless it still has stock held.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool hasStock = await db.Stock.AnyAsync(x => x.ItemId == id);
            if (hasStock)
            {
                ErrorMessage = "Can't delete an item that still has stock held.";
                return RedirectToPage();
            }

            Item? item = await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.ItemId == id);
            if (item is not null)
            {
                db.Items.Remove(item);
                await db.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
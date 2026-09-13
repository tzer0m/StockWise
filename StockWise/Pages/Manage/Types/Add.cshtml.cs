using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Types
{
    /// <summary>
    /// Page model for adding a new item type.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class AddModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// The new type.
        /// </summary>
        [BindProperty]
        public ItemType Type { get; set; } = new();

        /// <summary>
        /// Creates the new type.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Types.Add(Type);
            await db.SaveChangesAsync();
            await historyService.LogTypeAddedAsync(Type.Name);
            return RedirectToPage("Index");
        }
    }
}
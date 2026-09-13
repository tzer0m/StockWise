using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Manage.Types
{
    /// <summary>
    /// Page model for adding a new item type.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class AddModel(StockWiseDbContext db) : PageModel
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
            return RedirectToPage("Index");
        }
    }
}
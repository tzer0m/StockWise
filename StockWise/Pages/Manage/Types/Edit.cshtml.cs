using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Types
{
    /// <summary>
    /// Page model for renaming an item type.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class EditModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// The type being edited.
        /// </summary>
        [BindProperty]
        public ItemType Type { get; set; } = new();

        /// <summary>
        /// Loads the type to edit.
        /// </summary>
        /// <param name="id">The ID of the type to edit.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            ItemType? type = await db.Types.FindAsync(id);
            if (type is null)
            {
                return NotFound();
            }

            Type = type;
            return Page();
        }

        /// <summary>
        /// Saves the type's new name.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Attach(Type);
            db.Entry(Type).Property(x => x.Name).IsModified = true;
            await db.SaveChangesAsync();
            await historyService.LogTypeUpdatedAsync(Type.Name);
            return RedirectToPage("Index");
        }
    }
}
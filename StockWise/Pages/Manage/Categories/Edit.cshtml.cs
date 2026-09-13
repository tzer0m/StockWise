using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Categories
{
    /// <summary>
    /// Page model for renaming a storage category.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class EditModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// The category being edited.
        /// </summary>
        [BindProperty]
        public StorageCategory Category { get; set; } = new();

        /// <summary>
        /// Loads the category to edit.
        /// </summary>
        /// <param name="id">The ID of the category to edit.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            StorageCategory? category = await db.StorageCategories.FindAsync(id);
            if (category is null)
            {
                return NotFound();
            }

            Category = category;
            return Page();
        }

        /// <summary>
        /// Saves the category's new name.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Attach(Category);
            db.Entry(Category).Property(x => x.Name).IsModified = true;
            db.Entry(Category).Property(x => x.Color).IsModified = true;
            await db.SaveChangesAsync();
            await historyService.LogCategoryUpdatedAsync(Category.Name);
            return RedirectToPage("Index");
        }
    }
}
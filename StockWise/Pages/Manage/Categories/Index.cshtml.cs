using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Manage.Categories
{
    /// <summary>
    /// Page model for listing, adding, and deleting storage categories.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All storage categories, ordered by name, with their locations loaded.
        /// </summary>
        public List<StorageCategory> Categories { get; set; } = [];

        /// <summary>
        /// The name for a new category being added.
        /// </summary>
        [BindProperty]
        public string NewCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// An error message to show, if the last action failed.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loads all storage categories.
        /// </summary>
        public async Task OnGetAsync()
        {
            Categories = await db.StorageCategories.Include(x => x.Locations).OrderBy(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Adds a new storage category.
        /// </summary>
        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                db.StorageCategories.Add(new StorageCategory { Name = NewCategoryName.Trim() });
                await db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Deletes a storage category, unless it still has locations assigned to it.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool hasLocations = await db.Locations.AnyAsync(x => x.CategoryId == id);
            if (hasLocations)
            {
                ErrorMessage = "Can't delete a category that still has locations assigned to it.";
                return RedirectToPage();
            }

            StorageCategory? category = await db.StorageCategories.FindAsync(id);
            if (category is not null)
            {
                db.StorageCategories.Remove(category);
                await db.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
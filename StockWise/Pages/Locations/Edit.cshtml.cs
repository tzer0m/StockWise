using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Locations
{
    /// <summary>
    /// Page model for editing a storage location's name and category.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class EditModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// The location being edited.
        /// </summary>
        [BindProperty]
        public Location Location { get; set; } = new();

        /// <summary>
        /// The available categories, for the category dropdown.
        /// </summary>
        public List<SelectListItem> CategoryOptions { get; set; } = [];

        /// <summary>
        /// Loads the location to edit.
        /// </summary>
        /// <param name="id">The ID of the location to edit.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Location? location = await db.Locations.FindAsync(id);
            if (location is null)
            {
                return NotFound();
            }

            Location = location;
            await LoadCategoryOptionsAsync();
            return Page();
        }

        /// <summary>
        /// Saves the location's new name and category.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoryOptionsAsync();
                return Page();
            }

            EntityEntry<Location> entry = db.Attach(Location);
            entry.Property(x => x.Name).IsModified = true;
            entry.Property(x => x.CategoryId).IsModified = true;
            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Loads the category dropdown options from the database.
        /// </summary>
        private async Task LoadCategoryOptionsAsync()
        {
            List<StorageCategory> categories = await db.StorageCategories.OrderBy(x => x.Name).ToListAsync();
            CategoryOptions = categories.Select(x => new SelectListItem(x.Name, x.CategoryId.ToString())).ToList();
        }
    }
}
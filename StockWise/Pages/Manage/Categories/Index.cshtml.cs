using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Categories
{
    /// <summary>
    /// Page model for listing, adding, and deleting storage categories.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class IndexModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// All storage categories, ordered by name, with their locations loaded.
        /// </summary>
        public List<StorageCategory> Categories { get; set; } = [];

        /// <summary>
        /// The column to sort the categories table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "name";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "asc";

        /// <summary>
        /// The name for a new category being added.
        /// </summary>
        [BindProperty]
        public string NewCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// The highlight colour for a new category being added.
        /// </summary>
        [BindProperty]
        public string NewCategoryColor { get; set; } = "#6c757d";

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
            Categories = await ApplySort(db.StorageCategories.Include(x => x.Locations), Sort, Direction == "desc").ToListAsync();
        }

        /// <summary>
        /// Adds a new storage category.
        /// </summary>
        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                db.StorageCategories.Add(new StorageCategory { Name = NewCategoryName.Trim(), Color = NewCategoryColor });
                await db.SaveChangesAsync();
                await historyService.LogCategoryAddedAsync(NewCategoryName.Trim());
            }

            return RedirectToPage(new { Sort, Direction });
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
                return RedirectToPage(new { Sort, Direction });
            }

            StorageCategory? category = await db.StorageCategories.FindAsync(id);
            if (category is not null)
            {
                db.StorageCategories.Remove(category);
                await db.SaveChangesAsync();
                await historyService.LogCategoryDeletedAsync(category.Name);
            }

            return RedirectToPage(new { Sort, Direction });
        }

        /// <summary>
        /// Applies the requested sort to the categories query.
        /// </summary>
        /// <param name="query">The categories query to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static IOrderedQueryable<StorageCategory> ApplySort(IQueryable<StorageCategory> query, string sort, bool descending)
        {
            return sort switch
            {
                "color" => descending ? query.OrderByDescending(x => x.Color) : query.OrderBy(x => x.Color),
                "locations" => descending ? query.OrderByDescending(x => x.Locations.Count) : query.OrderBy(x => x.Locations.Count),
                _ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            };
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Manage.Locations
{
    /// <summary>
    /// Page model for listing, adding, and deleting storage locations.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// All storage locations, ordered by category then name.
        /// </summary>
        public List<Location> Locations { get; set; } = [];

        /// <summary>
        /// The column to sort the locations table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "category";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "asc";

        /// <summary>
        /// The available categories, for the add-location form.
        /// </summary>
        public List<SelectListItem> CategoryOptions { get; set; } = [];

        /// <summary>
        /// The category for a new location being added.
        /// </summary>
        [BindProperty]
        public int NewLocationCategoryId { get; set; }

        /// <summary>
        /// The name for a new location being added.
        /// </summary>
        [BindProperty]
        public string NewLocationName { get; set; } = string.Empty;

        /// <summary>
        /// An error message to show, if the last action failed.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loads all storage locations and the category options for the add form.
        /// </summary>
        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        /// <summary>
        /// Adds a new storage location.
        /// </summary>
        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewLocationName) && NewLocationCategoryId != 0)
            {
                db.Locations.Add(new Location { CategoryId = NewLocationCategoryId, Name = NewLocationName.Trim() });
                await db.SaveChangesAsync();
            }

            return RedirectToPage(new { Sort, Direction });
        }

        /// <summary>
        /// Deletes a storage location, unless it still has stock held there.
        /// </summary>
        /// <param name="id">The ID of the location to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool hasStock = await db.Stock.AnyAsync(x => x.LocationId == id);
            if (hasStock)
            {
                ErrorMessage = "Can't delete a location that still has stock held there.";
                return RedirectToPage(new { Sort, Direction });
            }

            Location? location = await db.Locations.FindAsync(id);
            if (location is not null)
            {
                db.Locations.Remove(location);
                await db.SaveChangesAsync();
            }

            return RedirectToPage(new { Sort, Direction });
        }

        /// <summary>
        /// Loads locations and category dropdown options from the database.
        /// </summary>
        private async Task LoadAsync()
        {
            Locations = await ApplySort(db.Locations.Include(x => x.Category), Sort, Direction == "desc").ToListAsync();
            List<StorageCategory> categories = await db.StorageCategories.OrderBy(x => x.Name).ToListAsync();
            CategoryOptions = [.. categories.Select(x => new SelectListItem(x.Name, x.CategoryId.ToString()))];
        }

        /// <summary>
        /// Applies the requested sort to the locations query.
        /// </summary>
        /// <param name="query">The locations query to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static IOrderedQueryable<Location> ApplySort(IQueryable<Location> query, string sort, bool descending)
        {
            return sort switch
            {
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                _ => descending ? query.OrderByDescending(x => x.Category!.Name).ThenBy(x => x.Name) : query.OrderBy(x => x.Category!.Name).ThenBy(x => x.Name),
            };
        }
    }
}
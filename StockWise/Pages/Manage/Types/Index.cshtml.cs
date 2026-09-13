using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Types
{
    /// <summary>
    /// Page model for listing and deleting item types.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class IndexModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// All item types, ordered by name.
        /// </summary>
        public List<ItemType> Types { get; set; } = [];

        /// <summary>
        /// The IDs of types that are still assigned to at least one item, and so can't be deleted.
        /// </summary>
        public HashSet<int> TypeIdsInUse { get; set; } = [];

        /// <summary>
        /// An error message to show, if the last action failed.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loads all item types.
        /// </summary>
        public async Task OnGetAsync()
        {
            Types = await db.Types.OrderBy(x => x.Name).ToListAsync();
            TypeIdsInUse = await db.Items.Where(x => x.TypeId != null).Select(x => x.TypeId!.Value).Distinct().ToHashSetAsync();
        }

        /// <summary>
        /// Deletes a type, unless it's still assigned to an item.
        /// </summary>
        /// <param name="id">The ID of the type to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool inUse = await db.Items.AnyAsync(x => x.TypeId == id);
            if (inUse)
            {
                ErrorMessage = "Can't delete a type that's still assigned to an item.";
                return RedirectToPage();
            }

            ItemType? type = await db.Types.FindAsync(id);
            if (type is not null)
            {
                db.Types.Remove(type);
                await db.SaveChangesAsync();
                await historyService.LogTypeDeletedAsync(type.Name);
            }

            return RedirectToPage();
        }
    }
}
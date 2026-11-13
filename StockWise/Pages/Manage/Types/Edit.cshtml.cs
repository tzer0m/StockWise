using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Types
{
    /// <summary>
    /// Page model for editing an item type's name and minimum quantity.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    /// <param name="shoppingListService">The shopping list service.</param>
    public class EditModel(StockWiseDbContext db, HistoryService historyService, ShoppingListService shoppingListService) : PageModel
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
        /// Saves the type's name and minimum quantity, adding it to the shopping list if it's now low on stock.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Attach(Type);
            db.Entry(Type).Property(x => x.Name).IsModified = true;
            db.Entry(Type).Property(x => x.MinQuantity).IsModified = true;
            await db.SaveChangesAsync();
            await historyService.LogTypeUpdatedAsync(Type.Name);
            await shoppingListService.CheckLowStockForTypeAsync(Type.TypeId);
            return RedirectToPage("Index");
        }
    }
}
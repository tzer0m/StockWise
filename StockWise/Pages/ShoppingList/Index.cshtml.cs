using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.ShoppingList
{
    /// <summary>
    /// Page model for viewing, adding to, and ticking off the shopping list.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="shoppingListService">The shopping list service.</param>
    public class IndexModel(StockWiseDbContext db, ShoppingListService shoppingListService) : PageModel
    {
        /// <summary>
        /// Every shopping list entry, ordered by name.
        /// </summary>
        public List<ShoppingListItem> Items { get; set; } = [];

        /// <summary>
        /// The total unopened stock for each type, keyed by type ID.
        /// </summary>
        public Dictionary<int, int> UnopenedByType { get; set; } = [];

        /// <summary>
        /// Every type name, offered as suggestions in the add box.
        /// </summary>
        public List<string> TypeNames { get; set; } = [];

        /// <summary>
        /// The name of a new entry being added.
        /// </summary>
        [BindProperty]
        public string NewItemName { get; set; } = string.Empty;

        /// <summary>
        /// Loads the list, current stock levels, and type suggestions.
        /// </summary>
        public async Task OnGetAsync()
        {
            Items = await shoppingListService.GetAllAsync();
            UnopenedByType = await db.Stock.Where(x => x.OpenedAt == null && x.Item!.TypeId != null).GroupBy(x => x.Item!.TypeId!.Value).Select(x => new { TypeId = x.Key, Total = x.Sum(y => y.Quantity) }).ToDictionaryAsync(x => x.TypeId, x => x.Total);
            TypeNames = await db.Types.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Adds a new entry, linked to a type if the name matches one.
        /// </summary>
        public async Task<IActionResult> OnPostAddAsync()
        {
            await shoppingListService.AddAsync(NewItemName);
            return RedirectToPage();
        }

        /// <summary>
        /// Ticks an entry off the list.
        /// </summary>
        /// <param name="id">The ID of the entry to tick off.</param>
        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            await shoppingListService.RemoveAsync(id);
            return RedirectToPage();
        }
    }
}
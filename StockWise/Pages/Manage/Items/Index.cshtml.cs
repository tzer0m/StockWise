using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Items
{
    /// <summary>
    /// Page model for listing and deleting trackable items, searching them by name, and scanning a barcode to check whether it's already known or add it if not.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="historyService">The history service.</param>
    public class IndexModel(StockWiseDbContext db, HistoryService historyService) : PageModel
    {
        /// <summary>
        /// All items, ordered by name.
        /// </summary>
        public List<Item> Items { get; set; } = [];

        /// <summary>
        /// The scanned barcode, or product name search text.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The column to sort the items table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "name";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "asc";

        /// <summary>
        /// Whether the scanned barcode already matches an existing item.
        /// </summary>
        public bool ItemAlreadyExists { get; set; }

        /// <summary>
        /// Whether the search bar text was a name search, so the items table is filtered to its matches.
        /// </summary>
        public bool IsSearchResults { get; set; }

        /// <summary>
        /// The IDs of items that still have stock held, and so can't be deleted.
        /// </summary>
        public HashSet<int> ItemIdsWithStock { get; set; } = [];

        /// <summary>
        /// An error message to show, if the last action failed.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Loads all items. If a barcode was scanned, shows a message when it already matches an item, otherwise filters the items by name, or goes straight into adding it when nothing matches and it looks like a barcode.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            Items = await ApplySort(db.Items.Include(x => x.Type), Sort, Direction == "desc").ToListAsync();
            ItemIdsWithStock = await db.Stock.Select(x => x.ItemId).Distinct().ToHashSetAsync();

            if (!string.IsNullOrWhiteSpace(Barcode))
            {
                if (await db.Items.AnyAsync(x => x.Barcode == Barcode))
                {
                    ItemAlreadyExists = true;
                }
                else
                {
                    string searchText = Barcode.Trim();
                    List<Item> matches = [.. Items.Where(x => ItemService.MatchesSearch(x, searchText))];
                    if (matches.Count == 0 && ItemService.LooksLikeBarcode(searchText))
                    {
                        return RedirectToPage("Add", new { barcode = searchText });
                    }
                    Items = matches;
                    IsSearchResults = true;
                }
            }

            return Page();
        }

        /// <summary>
        /// Deletes an item, unless it still has stock held.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            bool hasStock = await db.Stock.AnyAsync(x => x.ItemId == id);
            if (hasStock)
            {
                ErrorMessage = "Can't delete an item that still has stock held.";
                return RedirectToPage(new { Sort, Direction });
            }

            Item? item = await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.ItemId == id);
            if (item is not null)
            {
                db.Items.Remove(item);
                await db.SaveChangesAsync();
                await historyService.LogItemDeletedAsync(item.Name);
            }

            return RedirectToPage(new { Sort, Direction });
        }

        /// <summary>
        /// Applies the requested sort to the items query.
        /// </summary>
        /// <param name="query">The items query to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static IOrderedQueryable<Item> ApplySort(IQueryable<Item> query, string sort, bool descending)
        {
            return sort switch
            {
                "brand" => descending ? query.OrderByDescending(x => x.Brand) : query.OrderBy(x => x.Brand),
                "type" => descending ? query.OrderByDescending(x => x.Type!.Name) : query.OrderBy(x => x.Type!.Name),
                "barcode" => descending ? query.OrderByDescending(x => x.Barcode) : query.OrderBy(x => x.Barcode),
                "openable" => descending ? query.OrderByDescending(x => x.IsOpenable) : query.OrderBy(x => x.IsOpenable),
                _ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            };
        }
    }
}
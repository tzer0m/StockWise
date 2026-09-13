using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages
{
    /// <summary>
    /// Page model for the home page: a barcode scan hub, a set of type filter buttons, plus a single sortable table combining all current stock and frozen meal batches.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="mealService">The meal service.</param>
    public class IndexModel(StockWiseDbContext db, MealService mealService) : PageModel
    {
        /// <summary>
        /// All current stock and meal rows, sorted together and filtered by the selected types, if any.
        /// </summary>
        public List<IInventoryRow> AllRows { get; set; } = [];

        /// <summary>
        /// The scanned barcode.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The column to sort the combined table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "expiry";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "asc";

        /// <summary>
        /// The type names currently selected as filters. An empty list means no filter is applied.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public List<string> Types { get; set; } = [];

        /// <summary>
        /// Every distinct type name present in the unfiltered row set, used to render the filter buttons.
        /// </summary>
        public List<string> AvailableTypeNames { get; set; } = [];

        /// <summary>
        /// The item matching the scanned barcode, if found.
        /// </summary>
        public Item? ScannedItem { get; set; }

        /// <summary>
        /// The scanned item's current stock rows, with their locations loaded.
        /// </summary>
        public List<StockWise.Models.Stock> ScannedItemStock { get; set; } = [];

        /// <summary>
        /// The heading to show for the scanned item, formatted as "Brand Name", omitting the brand when it's unknown.
        /// </summary>
        public string ScannedItemTitle => ScannedItem is null
            ? string.Empty
            : string.IsNullOrWhiteSpace(ScannedItem.Brand)
                ? ScannedItem.Name
                : $"{ScannedItem.Brand} {ScannedItem.Name}";

        /// <summary>
        /// The meal instance matching the scanned barcode, if it parses as a guid and is found.
        /// </summary>
        public MealInstance? ScannedMealInstance { get; set; }

        /// <summary>
        /// A message to show after an action elsewhere redirects back here, such as adding stock.
        /// </summary>
        [TempData]
        public string? Message { get; set; }

        /// <summary>
        /// Loads the sorted, type-filtered combined stock/meal rows, plus the scanned item or meal instance if a barcode was given.
        /// </summary>
        public async Task OnGetAsync()
        {
            List<StockWise.Models.Stock> stockList = await db.Stock.Include(x => x.Item).ThenInclude(x => x!.Type).Include(x => x.Location).ThenInclude(x => x!.Category).ToListAsync();
            List<Meal> meals = await mealService.GetAllWithInstancesAsync();

            List<IInventoryRow> rows = [.. stockList.Select(x => new StockRow(x)), .. meals.Select(x => new MealRow(x))];
            AvailableTypeNames = [.. rows.Select(x => x.TypeName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x)];

            List<IInventoryRow> filteredRows = Types.Count == 0 ? rows : [.. rows.Where(x => Types.Contains(x.TypeName))];
            AllRows = ApplySort(filteredRows, Sort, Direction == "desc");

            await LoadScannedItemAsync();
        }

        /// <summary>
        /// Checks out one unit of stock from a row, deleting it if it reaches zero.
        /// </summary>
        /// <param name="stockId">The ID of the stock row to check out from.</param>
        public async Task<IActionResult> OnPostCheckoutAsync(int stockId)
        {
            StockWise.Models.Stock? stock = await db.Stock.FindAsync(stockId);
            if (stock is not null)
            {
                stock.Quantity--;
                if (stock.Quantity <= 0)
                {
                    db.Stock.Remove(stock);
                }

                await db.SaveChangesAsync();
            }

            return Redirect(BuildIndexUrl(includeBarcode: true, Types));
        }

        /// <summary>
        /// Checks out multiple units of stock from a row in one action, deleting it if it reaches zero.
        /// </summary>
        /// <param name="stockId">The ID of the stock row to check out from.</param>
        /// <param name="quantity">The number of units to check out.</param>
        public async Task<IActionResult> OnPostCheckoutMultipleAsync(int stockId, int quantity)
        {
            StockWise.Models.Stock? stock = await db.Stock.FindAsync(stockId);
            if (stock is not null && quantity > 0)
            {
                stock.Quantity -= Math.Min(quantity, stock.Quantity);
                if (stock.Quantity <= 0)
                {
                    db.Stock.Remove(stock);
                }

                await db.SaveChangesAsync();
            }

            return Redirect(BuildIndexUrl(includeBarcode: true, Types));
        }

        /// <summary>
        /// Finishes an opened stock row, removing it entirely.
        /// </summary>
        /// <param name="stockId">The ID of the stock row to finish.</param>
        public async Task<IActionResult> OnPostFinishAsync(int stockId)
        {
            StockWise.Models.Stock? stock = await db.Stock.FindAsync(stockId);
            if (stock is not null)
            {
                db.Stock.Remove(stock);
                await db.SaveChangesAsync();
            }

            return Redirect(BuildIndexUrl(includeBarcode: true, Types));
        }

        /// <summary>
        /// Eats one meal instance, always consuming the whole thing, and deletes its parent batch too if it was the last remaining instance. Always clears the scanned barcode on redirect, since a single-use instance guid no longer resolves to anything once eaten.
        /// </summary>
        /// <param name="mealInstanceId">The guid of the instance to eat.</param>
        public async Task<IActionResult> OnPostEatAsync(Guid mealInstanceId)
        {
            await mealService.EatAsync(mealInstanceId);
            return Redirect(BuildIndexUrl(includeBarcode: false, Types));
        }

        /// <summary>
        /// Builds the URL for a type filter button: toggles the given type in or out of the current selection while preserving the barcode, sort, and direction, or clears every filter when no type is given, for the Reset button.
        /// </summary>
        /// <param name="typeName">The type to toggle, or null to reset every filter.</param>
        public string BuildFilterUrl(string? typeName)
        {
            List<string> types = typeName is null ? [] : Types.Contains(typeName) ? [.. Types.Where(x => x != typeName)] : [.. Types, typeName];
            return BuildIndexUrl(includeBarcode: true, types);
        }

        /// <summary>
        /// Builds the URL for a column sort header: switches to the given column, toggling direction if it's already the active column, while preserving the barcode and selected type filters.
        /// </summary>
        /// <param name="column">The column to sort by.</param>
        public string BuildSortUrl(string column)
        {
            string direction = Sort == column && Direction == "asc" ? "desc" : "asc";
            return BuildIndexUrl(includeBarcode: true, Types, column, direction);
        }

        /// <summary>
        /// Builds a URL back to the home page, manually assembling the query string so the type filter list survives as repeated "Types" parameters, which ASP.NET Core's route-value-based URL generation doesn't produce for a list value.
        /// </summary>
        /// <param name="includeBarcode">Whether to carry the current scanned barcode over.</param>
        /// <param name="types">The type filters to include.</param>
        /// <param name="sort">The sort column to use, or the current one if not given.</param>
        /// <param name="direction">The sort direction to use, or the current one if not given.</param>
        private string BuildIndexUrl(bool includeBarcode, List<string> types, string? sort = null, string? direction = null)
        {
            List<string> queryParts = [];
            if (includeBarcode && !string.IsNullOrWhiteSpace(Barcode))
            {
                queryParts.Add($"Barcode={Uri.EscapeDataString(Barcode)}");
            }

            queryParts.Add($"Sort={Uri.EscapeDataString(sort ?? Sort)}");
            queryParts.Add($"Direction={Uri.EscapeDataString(direction ?? Direction)}");
            queryParts.AddRange(types.Select(x => $"Types={Uri.EscapeDataString(x)}"));

            return "/Index?" + string.Join("&", queryParts);
        }

        /// <summary>
        /// Applies the requested sort, always pushing rows with no expiry date to the end regardless of direction.
        /// </summary>
        /// <param name="rows">The combined stock/meal rows to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static List<IInventoryRow> ApplySort(List<IInventoryRow> rows, string sort, bool descending)
        {
            IOrderedEnumerable<IInventoryRow> ordered = sort switch
            {
                "item" => descending ? rows.OrderByDescending(x => x.Name) : rows.OrderBy(x => x.Name),
                "brand" => descending ? rows.OrderByDescending(x => x.Brand) : rows.OrderBy(x => x.Brand),
                "type" => descending ? rows.OrderByDescending(x => x.TypeName) : rows.OrderBy(x => x.TypeName),
                "location" => descending ? rows.OrderByDescending(x => x.LocationName) : rows.OrderBy(x => x.LocationName),
                "quantity" => descending ? rows.OrderByDescending(x => x.Quantity) : rows.OrderBy(x => x.Quantity),
                "opened" => descending ? rows.OrderByDescending(x => x.OpenedAt) : rows.OrderBy(x => x.OpenedAt),
                _ => descending ? rows.OrderBy(x => x.Expiry == null).ThenByDescending(x => x.Expiry) : rows.OrderBy(x => x.Expiry == null).ThenBy(x => x.Expiry),
            };

            return [.. ordered];
        }

        /// <summary>
        /// Calculates the number of days from today until the given expiry date.
        /// </summary>
        /// <param name="expiry">The expiry date.</param>
        public static int? GetDaysUntilExpiry(DateOnly? expiry)
        {
            return expiry?.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        }

        /// <summary>
        /// Returns the CSS class for the expiry badge, or null if the row doesn't need highlighting.
        /// </summary>
        /// <param name="daysUntilExpiry">The number of days until expiry.</param>
        public static string? GetExpiryBadgeClass(int? daysUntilExpiry)
        {
            if (daysUntilExpiry is null)
            {
                return null;
            }

            if (daysUntilExpiry <= 0)
            {
                return "tile tile-red";
            }

            if (daysUntilExpiry < 3)
            {
                return "tile tile-orange";
            }

            if (daysUntilExpiry < 7)
            {
                return "tile tile-green";
            }

            return null;
        }

        /// <summary>
        /// Formats the number of days until expiry as a short display string.
        /// </summary>
        /// <param name="daysUntilExpiry">The number of days until expiry.</param>
        public static string FormatDaysUntilExpiry(int? daysUntilExpiry)
        {
            if (daysUntilExpiry is null)
            {
                return "—";
            }

            if (daysUntilExpiry == 0)
            {
                return "Today";
            }

            if (daysUntilExpiry < 0)
            {
                return $"{-daysUntilExpiry}d overdue";
            }

            if (daysUntilExpiry <= 6)
            {
                return $"{daysUntilExpiry}d";
            }

            int weeks = (int)Math.Round(daysUntilExpiry.Value / 7.0);
            if (weeks <= 4)
            {
                return $"{weeks}w";
            }

            int months = (int)Math.Round(daysUntilExpiry.Value / 30.44);
            if (months <= 12)
            {
                return $"{months}mo";
            }

            int years = (int)Math.Round(daysUntilExpiry.Value / 365.25);
            return $"{years}y";
        }

        /// <summary>
        /// Looks up the item for the scanned barcode and its current stock rows, or - if no item matches and the barcode parses as a guid - the meal instance it identifies, if any.
        /// </summary>
        private async Task LoadScannedItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                return;
            }

            ScannedItem = await db.Items.Include(x => x.Type).FirstOrDefaultAsync(x => x.Barcode == Barcode);
            if (ScannedItem is not null)
            {
                ScannedItemStock = await db.Stock.Include(x => x.Location).Where(x => x.ItemId == ScannedItem.ItemId).OrderBy(x => x.Location!.Name).ToListAsync();
                return;
            }

            if (Guid.TryParse(Barcode, out Guid mealInstanceId))
            {
                ScannedMealInstance = await mealService.FindInstanceAsync(mealInstanceId);
            }
        }
    }
}
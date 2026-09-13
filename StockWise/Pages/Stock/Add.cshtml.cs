using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Stock
{
    /// <summary>
    /// Page model for scanning an item's barcode and adding stock for it.
    /// </summary>
    /// <param name="itemService">The item service.</param>
    /// <param name="stockService">The stock service.</param>
    public class AddModel(ItemService itemService, StockService stockService) : PageModel
    {
        /// <summary>
        /// The scanned barcode.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Barcode { get; set; }

        /// <summary>
        /// The item matching the scanned barcode, if found.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// The locations allowed for this item while unopened.
        /// </summary>
        public List<Location> AllowedLocations { get; set; } = [];

        /// <summary>
        /// The location to add stock at.
        /// </summary>
        [BindProperty]
        public int LocationId { get; set; }

        /// <summary>
        /// The quantity to add.
        /// </summary>
        [BindProperty]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The expiry date for the added stock, if any.
        /// </summary>
        [BindProperty]
        public DateOnly? Expiry { get; set; }

        /// <summary>
        /// A message to show after adding stock.
        /// </summary>
        [TempData]
        public string? Message { get; set; }

        /// <summary>
        /// Looks up the item for the scanned barcode, if any.
        /// </summary>
        public async Task OnGetAsync()
        {
            await LoadItemAsync();
        }

        /// <summary>
        /// Adds stock for the scanned item, merging with a matching existing row if one exists.
        /// </summary>
        /// <param name="itemId">The ID of the item to add stock for.</param>
        public async Task<IActionResult> OnPostAddStockAsync(int itemId)
        {
            await stockService.AddOrMergeAsync(itemId, LocationId, Quantity, Expiry);
            Message = "Stock added.";
            return RedirectToPage();
        }

        /// <summary>
        /// Loads the scanned item and its allowed unopened locations.
        /// </summary>
        private async Task LoadItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                return;
            }

            Item = await itemService.FindByBarcodeAsync(Barcode);
            if (Item is null)
            {
                return;
            }

            AllowedLocations = await itemService.GetAllowedLocationsAsync(Item);
        }
    }
}
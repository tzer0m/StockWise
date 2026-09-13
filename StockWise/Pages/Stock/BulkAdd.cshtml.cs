using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Stock
{
    /// <summary>
    /// Page model for scanning a batch of barcodes, then stepping through each one to add its stock or create it if unknown.
    /// </summary>
    /// <param name="itemService">The item service.</param>
    /// <param name="stockService">The stock service.</param>
    public class BulkAddModel(ItemService itemService, StockService stockService) : PageModel
    {
        /// <summary>
        /// The remaining barcode/quantity pairs still to process, encoded as "barcode|quantity" entries separated by commas.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Batch { get; set; }

        /// <summary>
        /// The barcode currently being processed.
        /// </summary>
        public string? CurrentBarcode { get; set; }

        /// <summary>
        /// How many items are left in the batch, including the one currently being processed.
        /// </summary>
        public int RemainingCount { get; set; }

        /// <summary>
        /// The item matching the current barcode, if known.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// The locations allowed for the current item while unopened.
        /// </summary>
        public List<Location> AllowedLocations { get; set; } = [];

        /// <summary>
        /// A JSON lookup of every item's barcode and name, embedded into the scan page for instant client-side matching.
        /// </summary>
        public string ItemLookupJson { get; set; } = "[]";

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
        /// The barcode for a new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public string NewItemBarcode { get; set; } = string.Empty;

        /// <summary>
        /// The display name for a new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public string NewItemName { get; set; } = string.Empty;

        /// <summary>
        /// The brand for the new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public string? NewItemBrand { get; set; }

        /// <summary>
        /// A URL to an image of the new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public string? NewItemImageUrl { get; set; }

        /// <summary>
        /// Whether the new item can be opened, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public bool NewItemIsOpenable { get; set; }

        /// <summary>
        /// How many days after opening the new item expires, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public int? NewItemExpiryAfterOpeningDays { get; set; }

        /// <summary>
        /// The category allowances chosen for the new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public List<CategoryAllowance> NewItemCategoryAllowances { get; set; } = [];

        /// <summary>
        /// Loads the scan page if no batch is in progress, otherwise the next item in the batch to review.
        /// </summary>
        public async Task OnGetAsync()
        {
            List<(string Barcode, int Quantity)> entries = ParseEntries(Batch);
            if (entries.Count == 0)
            {
                await LoadItemLookupAsync();
                return;
            }

            await LoadCurrentItemAsync(entries);
        }

        /// <summary>
        /// Adds stock for the current known item, merging with a matching existing row if one exists, and advances to the next item in the batch.
        /// </summary>
        /// <param name="itemId">The ID of the item to add stock for.</param>
        public async Task<IActionResult> OnPostAddStockAsync(int itemId)
        {
            List<(string Barcode, int Quantity)> entries = ParseEntries(Batch);
            if (entries.Count == 0)
            {
                return RedirectToPage("/Index");
            }

            await stockService.AddOrMergeAsync(itemId, LocationId, Quantity, Expiry);
            return AdvanceToNext(entries);
        }

        /// <summary>
        /// Creates the new item for the current unknown barcode, then re-shows this step so its stock can be added.
        /// </summary>
        public async Task<IActionResult> OnPostAddItemAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            await itemService.CreateAsync(NewItemBarcode, NewItemName, NewItemBrand, NewItemImageUrl, NewItemIsOpenable, NewItemExpiryAfterOpeningDays, NewItemCategoryAllowances);
            return RedirectToPage(new { Batch });
        }

        /// <summary>
        /// Skips the current item without adding stock, and advances to the next item in the batch.
        /// </summary>
        public IActionResult OnPostSkip()
        {
            List<(string Barcode, int Quantity)> entries = ParseEntries(Batch);
            return entries.Count == 0 ? RedirectToPage("/Index") : AdvanceToNext(entries);
        }

        /// <summary>
        /// Loads the current item in the batch, its allowed locations if known, or its new-item category allowances if not.
        /// </summary>
        /// <param name="entries">The parsed batch entries.</param>
        private async Task LoadCurrentItemAsync(List<(string Barcode, int Quantity)> entries)
        {
            (string Barcode, int Quantity) current = entries[0];
            CurrentBarcode = current.Barcode;
            RemainingCount = entries.Count;
            Quantity = current.Quantity;

            Item = await itemService.FindByBarcodeAsync(current.Barcode);
            if (Item is not null)
            {
                AllowedLocations = await itemService.GetAllowedLocationsAsync(Item);
                return;
            }

            NewItemBarcode = current.Barcode;
            NewItemCategoryAllowances = await itemService.GetCategoryAllowancesAsync();
        }

        /// <summary>
        /// Loads every item's barcode and name as JSON, for instant client-side matching while scanning.
        /// </summary>
        private async Task LoadItemLookupAsync()
        {
            List<BarcodeNameLookup> lookup = await itemService.GetBarcodeNameLookupAsync();
            ItemLookupJson = JsonSerializer.Serialize(lookup);
        }

        /// <summary>
        /// Redirects to the next item in the batch, or to the home page if none remain.
        /// </summary>
        /// <param name="entries">The parsed batch entries, including the one just processed.</param>
        private RedirectToPageResult AdvanceToNext(List<(string Barcode, int Quantity)> entries)
        {
            string remaining = string.Join(',', entries.Skip(1).Select(x => $"{x.Barcode}|{x.Quantity}"));
            return string.IsNullOrEmpty(remaining) ? RedirectToPage("/Index") : RedirectToPage(new { Batch = remaining });
        }

        /// <summary>
        /// Splits a batch string into its barcode/quantity entries.
        /// </summary>
        /// <param name="batch">The batch string to parse.</param>
        private static List<(string Barcode, int Quantity)> ParseEntries(string? batch)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                return [];
            }

            List<(string Barcode, int Quantity)> entries = [];
            foreach (string part in batch.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pieces = part.Split('|');
                if (pieces.Length == 2 && int.TryParse(pieces[1], out int quantity))
                {
                    entries.Add((pieces[0], quantity));
                }
            }

            return entries;
        }
    }
}
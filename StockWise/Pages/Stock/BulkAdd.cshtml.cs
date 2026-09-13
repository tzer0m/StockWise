using System.ComponentModel.DataAnnotations;
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
        [Range(1, int.MaxValue, ErrorMessage = "Select a location.")]
        public int LocationId { get; set; }

        /// <summary>
        /// The quantity to add.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a quantity of at least 1.")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The expiry date for the added stock, if any.
        /// </summary>
        [BindProperty]
        public DateOnly? Expiry { get; set; }

        /// <summary>
        /// The fields for a new item, when the current barcode is unknown.
        /// </summary>
        [BindProperty]
        public ItemFormInput NewItem { get; set; } = new();

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

            RemoveModelStateExcept(nameof(LocationId), nameof(Quantity), nameof(Expiry));

            if (!ModelState.IsValid)
            {
                await LoadDisplayContextAsync(entries);
                return Page();
            }

            await stockService.AddOrMergeAsync(itemId, LocationId, Quantity, Expiry);
            return AdvanceToNext(entries);
        }

        /// <summary>
        /// Creates the new item for the current unknown barcode, then re-shows this step so its stock can be added.
        /// </summary>
        public async Task<IActionResult> OnPostAddItemAsync()
        {
            List<(string Barcode, int Quantity)> entries = ParseEntries(Batch);

            RemoveModelStateExcept(nameof(NewItem));

            if (!ModelState.IsValid)
            {
                if (entries.Count > 0)
                {
                    await LoadDisplayContextAsync(entries);
                }

                return Page();
            }

            await itemService.CreateAsync(NewItem);
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

            NewItem = new ItemFormInput { Barcode = current.Barcode, CategoryAllowances = await itemService.GetCategoryAllowancesAsync() };
        }

        /// <summary>
        /// Removes every ModelState entry that isn't for one of the given top-level properties (or a nested member of one), so validation errors from a different form on this page can't block submission of this one.
        /// </summary>
        /// <param name="propertyNames">The top-level bound property names whose entries, and nested members, should be kept.</param>
        private void RemoveModelStateExcept(params string[] propertyNames)
        {
            foreach (string key in ModelState.Keys.Where(x => !propertyNames.Any(p => x == p || x.StartsWith(p + ".") || x.StartsWith(p + "["))).ToList())
            {
                ModelState.Remove(key);
            }
        }

        /// <summary>
        /// Reloads just the current barcode, remaining count, and matching item/locations, for redisplaying this step after a failed validation without overwriting any of the values just posted.
        /// </summary>
        /// <param name="entries">The parsed batch entries.</param>
        private async Task LoadDisplayContextAsync(List<(string Barcode, int Quantity)> entries)
        {
            (string Barcode, int Quantity) current = entries[0];
            CurrentBarcode = current.Barcode;
            RemainingCount = entries.Count;

            Item = await itemService.FindByBarcodeAsync(current.Barcode);
            if (Item is not null)
            {
                AllowedLocations = await itemService.GetAllowedLocationsAsync(Item);
            }
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
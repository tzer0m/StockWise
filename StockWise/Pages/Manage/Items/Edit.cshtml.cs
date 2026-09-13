using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Items
{
    /// <summary>
    /// Page model for editing a trackable item and its category allowances.
    /// </summary>
    /// <param name="itemService">The item service.</param>
    public class EditModel(ItemService itemService) : PageModel
    {
        /// <summary>
        /// The ID of the item being edited.
        /// </summary>
        [BindProperty]
        public int ItemId { get; set; }

        /// <summary>
        /// The item's barcode.
        /// </summary>
        [BindProperty]
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// The item's display name.
        /// </summary>
        [BindProperty]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The item's brand, if known.
        /// </summary>
        [BindProperty]
        public string? Brand { get; set; }

        /// <summary>
        /// A URL to an image of the item.
        /// </summary>
        [BindProperty]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Whether a unit of this item can be opened.
        /// </summary>
        [BindProperty]
        public bool IsOpenable { get; set; }

        /// <summary>
        /// How many days after opening this item expires, used to default the expiry date when it's opened.
        /// </summary>
        [BindProperty]
        public int? ExpiryAfterOpeningDays { get; set; }

        /// <summary>
        /// The category allowances the user has chosen for this item.
        /// </summary>
        [BindProperty]
        public List<CategoryAllowance> CategoryAllowances { get; set; } = [];

        /// <summary>
        /// Loads the item to edit.
        /// </summary>
        /// <param name="id">The ID of the item to edit.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Item? item = await itemService.FindByIdAsync(id);
            if (item is null)
            {
                return NotFound();
            }

            ItemId = item.ItemId;
            Barcode = item.Barcode;
            Name = item.Name;
            Brand = item.Brand;
            ImageUrl = item.ImageUrl;
            IsOpenable = item.IsOpenable;
            ExpiryAfterOpeningDays = item.ExpiryAfterOpeningDays;
            CategoryAllowances = await itemService.GetCategoryAllowancesAsync(item);
            return Page();
        }

        /// <summary>
        /// Saves the item's details and category allowances.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Item? item = await itemService.FindByIdAsync(ItemId);
            if (item is null)
            {
                return NotFound();
            }

            await itemService.UpdateAsync(item, Barcode, Name, Brand, ImageUrl, IsOpenable, ExpiryAfterOpeningDays, CategoryAllowances);
            return RedirectToPage("Index");
        }
    }
}
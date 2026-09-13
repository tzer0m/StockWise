using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Manage.Items
{
    /// <summary>
    /// Page model for adding a new trackable item.
    /// </summary>
    /// <param name="itemService">The item service.</param>
    public class AddModel(ItemService itemService) : PageModel
    {
        /// <summary>
        /// The barcode for the new item.
        /// </summary>
        [BindProperty]
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// The display name for the new item.
        /// </summary>
        [BindProperty]
        public string Name { get; set; } = string.Empty;

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
        /// Loads the form with the barcode prefilled, if one was supplied.
        /// </summary>
        /// <param name="barcode">A barcode to prefill, e.g. from a failed stock lookup.</param>
        public async Task OnGetAsync(string? barcode)
        {
            Barcode = barcode ?? string.Empty;
            CategoryAllowances = await itemService.GetCategoryAllowancesAsync();
        }

        /// <summary>
        /// Creates the new item and its category allowances.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Item item = await itemService.CreateAsync(Barcode, Name, ImageUrl, IsOpenable, ExpiryAfterOpeningDays, CategoryAllowances);
            return RedirectToPage("/Stock/Add", new { barcode = item.Barcode });
        }
    }
}
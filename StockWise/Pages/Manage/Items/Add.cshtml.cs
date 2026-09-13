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
        /// The fields for the new item.
        /// </summary>
        [BindProperty]
        public ItemFormInput Input { get; set; } = new();

        /// <summary>
        /// Loads the form with the barcode prefilled, if one was supplied.
        /// </summary>
        /// <param name="barcode">A barcode to prefill, e.g. from a failed stock lookup.</param>
        public async Task OnGetAsync(string? barcode)
        {
            Input = new ItemFormInput { Barcode = barcode ?? string.Empty, CategoryAllowances = await itemService.GetCategoryAllowancesAsync() };
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

            Item item = await itemService.CreateAsync(Input);
            return RedirectToPage("/Stock/Add", new { barcode = item.Barcode });
        }
    }
}
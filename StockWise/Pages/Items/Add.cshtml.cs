using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Items
{
    /// <summary>
    /// Page model for adding a new trackable item.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class AddModel(StockWiseDbContext db) : PageModel
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
            await LoadCategoryAllowancesAsync();
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

            Item item = new() { Barcode = Barcode.Trim(), Name = Name.Trim(), ImageUrl = ImageUrl, IsOpenable = IsOpenable, CreatedAt = DateTime.UtcNow };
            foreach (CategoryAllowance allowance in CategoryAllowances.Where(x => x.AllowedWhenUnopened || x.AllowedWhenOpened))
            {
                item.ItemStorageCategories.Add(new ItemStorageCategory { CategoryId = allowance.CategoryId, AllowedWhenUnopened = allowance.AllowedWhenUnopened, AllowedWhenOpened = allowance.AllowedWhenOpened });
            }

            db.Items.Add(item);
            await db.SaveChangesAsync();
            return RedirectToPage("/Stock/Add", new { barcode = item.Barcode });
        }

        /// <summary>
        /// Loads the category allowance rows from the database.
        /// </summary>
        private async Task LoadCategoryAllowancesAsync()
        {
            List<StorageCategory> categories = await db.StorageCategories.OrderBy(x => x.Name).ToListAsync();
            CategoryAllowances = categories.Select(x => new CategoryAllowance { CategoryId = x.CategoryId, CategoryName = x.Name }).ToList();
        }
    }
}
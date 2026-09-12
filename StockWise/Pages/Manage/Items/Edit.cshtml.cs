using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Pages.Manage.Items
{
    /// <summary>
    /// Page model for editing a trackable item and its category allowances.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class EditModel(StockWiseDbContext db) : PageModel
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
            Item? item = await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.ItemId == id);
            if (item is null)
            {
                return NotFound();
            }

            ItemId = item.ItemId;
            Barcode = item.Barcode;
            Name = item.Name;
            ImageUrl = item.ImageUrl;
            IsOpenable = item.IsOpenable;
            ExpiryAfterOpeningDays = item.ExpiryAfterOpeningDays;
            await LoadCategoryAllowancesAsync(item);
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

            Item? item = await db.Items.Include(x => x.ItemStorageCategories).FirstOrDefaultAsync(x => x.ItemId == ItemId);
            if (item is null)
            {
                return NotFound();
            }

            item.Barcode = Barcode.Trim();
            item.Name = Name.Trim();
            item.ImageUrl = ImageUrl;
            item.IsOpenable = IsOpenable;
            item.ExpiryAfterOpeningDays = IsOpenable ? ExpiryAfterOpeningDays : null;

            item.ItemStorageCategories.Clear();
            foreach (CategoryAllowance allowance in CategoryAllowances.Where(x => x.AllowedWhenUnopened || (IsOpenable && x.AllowedWhenOpened)))
            {
                item.ItemStorageCategories.Add(new ItemStorageCategory { ItemId = item.ItemId, CategoryId = allowance.CategoryId, AllowedWhenUnopened = allowance.AllowedWhenUnopened, AllowedWhenOpened = IsOpenable && allowance.AllowedWhenOpened });
            }

            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Loads the category allowance rows from the database, marking any the item already has.
        /// </summary>
        /// <param name="item">The item being edited.</param>
        private async Task LoadCategoryAllowancesAsync(Item item)
        {
            List<StorageCategory> categories = await db.StorageCategories.OrderBy(x => x.Name).ToListAsync();
            CategoryAllowances = [.. categories.Select(x => new CategoryAllowance { CategoryId = x.CategoryId, CategoryName = x.Name, AllowedWhenUnopened = item.ItemStorageCategories.Any(y => y.CategoryId == x.CategoryId && y.AllowedWhenUnopened), AllowedWhenOpened = item.ItemStorageCategories.Any(y => y.CategoryId == x.CategoryId && y.AllowedWhenOpened) })];
        }
    }
}
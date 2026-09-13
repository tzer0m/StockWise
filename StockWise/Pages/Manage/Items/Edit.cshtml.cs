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
        /// The item's fields.
        /// </summary>
        [BindProperty]
        public ItemFormInput Input { get; set; } = new();

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
            Input = new ItemFormInput { Barcode = item.Barcode, Name = item.Name, Brand = item.Brand, TypeId = item.TypeId ?? 0, ImageUrl = item.ImageUrl, IsOpenable = item.IsOpenable, ExpiryAfterOpeningDays = item.ExpiryAfterOpeningDays, CategoryAllowances = await itemService.GetCategoryAllowancesAsync(item) };
            ViewData["AvailableTypes"] = await itemService.GetTypesAsync();
            return Page();
        }

        /// <summary>
        /// Saves the item's details and category allowances.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["AvailableTypes"] = await itemService.GetTypesAsync();
                return Page();
            }

            Item? item = await itemService.FindByIdAsync(ItemId);
            if (item is null)
            {
                return NotFound();
            }

            await itemService.UpdateAsync(item, Input);
            return RedirectToPage("Index");
        }
    }
}
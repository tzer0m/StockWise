using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Meals
{
    /// <summary>
    /// Page model for creating a new frozen meal batch and printing its tagged labels.
    /// </summary>
    /// <param name="mealService">The meal service.</param>
    public class AddModel(MealService mealService) : PageModel
    {
        /// <summary>
        /// The meal's display name.
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Enter a name.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The freezer location to store the batch at.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Select a location.")]
        public int LocationId { get; set; }

        /// <summary>
        /// The number of individually-tagged instances to create.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a quantity of at least 1.")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// The freezer locations a batch can be stored at.
        /// </summary>
        public List<Location> FreezerLocations { get; set; } = [];

        /// <summary>
        /// A message to show if the print job fails, so nothing was saved.
        /// </summary>
        public string? PrintError { get; set; }

        /// <summary>
        /// A message to show on the home page after adding a meal batch.
        /// </summary>
        [TempData]
        public string? Message { get; set; }

        /// <summary>
        /// Loads the freezer locations available to store a new batch at.
        /// </summary>
        public async Task OnGetAsync()
        {
            FreezerLocations = await mealService.GetFreezerLocationsAsync();
        }

        /// <summary>
        /// Creates the meal batch, printing one tagged label per instance first - nothing is saved if the print job fails.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                FreezerLocations = await mealService.GetFreezerLocationsAsync();
                return Page();
            }

            string name = Name.Trim();
            bool added = await mealService.AddAsync(name, LocationId, Quantity);
            if (!added)
            {
                PrintError = "Could not reach the printer, so nothing was saved. Check the printer and try again.";
                FreezerLocations = await mealService.GetFreezerLocationsAsync();
                return Page();
            }

            Message = $"{name} added.";
            return RedirectToPage("/Index");
        }
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Pages.Meals
{
    /// <summary>
    /// Page model for moving some or all of a frozen meal batch to a different freezer location.
    /// </summary>
    /// <param name="mealService">The meal service.</param>
    public class MoveModel(MealService mealService) : PageModel
    {
        /// <summary>
        /// The meal batch being moved.
        /// </summary>
        public Meal? Meal { get; set; }

        /// <summary>
        /// The freezer locations this batch can be moved to, excluding its current one.
        /// </summary>
        public List<Location> AllowedLocations { get; set; } = [];

        /// <summary>
        /// The scanned instance to move first, if the move was started from a scan.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? InstanceId { get; set; }

        /// <summary>
        /// The location to move the meals to.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Select a location.")]
        public int LocationId { get; set; }

        /// <summary>
        /// The number of instances to move.
        /// </summary>
        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a quantity of at least 1.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Loads the batch to move and its allowed destination locations, defaulting the quantity to 1 when started from a scanned instance, or the whole batch otherwise.
        /// </summary>
        /// <param name="id">The ID of the batch to move.</param>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!await LoadMealAsync(id))
            {
                return NotFound();
            }

            Quantity = InstanceId is not null && Meal!.Instances.Any(x => x.MealInstanceId == InstanceId) ? 1 : Meal!.Instances.Count;
            return Page();
        }

        /// <summary>
        /// Moves the chosen number of instances to the new location.
        /// </summary>
        /// <param name="id">The ID of the batch to move.</param>
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!await LoadMealAsync(id))
            {
                return NotFound();
            }

            if (Quantity > Meal!.Instances.Count)
            {
                ModelState.AddModelError(nameof(Quantity), $"Only {Meal.Instances.Count} available to move.");
            }

            if (!AllowedLocations.Any(x => x.LocationId == LocationId))
            {
                ModelState.AddModelError(nameof(LocationId), "Select a valid location.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await mealService.MoveAsync(Meal, LocationId, Quantity, InstanceId);
            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Loads the batch and the freezer locations it can be moved to.
        /// </summary>
        /// <param name="id">The ID of the batch to load.</param>
        private async Task<bool> LoadMealAsync(int id)
        {
            Meal = await mealService.FindAsync(id);
            if (Meal is null)
            {
                return false;
            }

            int currentLocationId = Meal.LocationId;
            List<Location> freezerLocations = await mealService.GetFreezerLocationsAsync();
            AllowedLocations = [.. freezerLocations.Where(x => x.LocationId != currentLocationId)];
            return true;
        }
    }
}
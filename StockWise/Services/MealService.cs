using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Provides meal-batch creation, lookup, and consumption logic for the frozen-meals feature.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="chitterClient">The client used to print meal labels.</param>
    /// <param name="historyService">The history service.</param>
    public class MealService(StockWiseDbContext db, ChitterClient chitterClient, HistoryService historyService)
    {
        /// <summary>
        /// Returns the freezer locations a new meal batch can be stored at, ordered by name.
        /// </summary>
        public async Task<List<Location>> GetFreezerLocationsAsync()
        {
            return await db.Locations.Include(x => x.Category).Where(x => x.Category!.Name == "Freezer").OrderBy(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Creates a new meal batch and prints one tagged label per instance, only saving to the database if the print job succeeds.
        /// </summary>
        /// <param name="name">The meal's display name.</param>
        /// <param name="locationId">The freezer location to store the batch at.</param>
        /// <param name="quantity">The number of individually-tagged instances to create.</param>
        /// <returns>True if the batch was printed and saved, false if the print job failed.</returns>
        public async Task<bool> AddAsync(string name, int locationId, int quantity)
        {
            List<Guid> guids = [.. Enumerable.Range(0, quantity).Select(_ => Guid.NewGuid())];

            bool printed = await chitterClient.PrintMealLabelsAsync(name, guids);
            if (!printed)
            {
                return false;
            }

            DateOnly frozenAt = DateOnly.FromDateTime(DateTime.UtcNow);
            Meal meal = new() { Name = name, LocationId = locationId, FrozenAt = frozenAt, Expiry = frozenAt.AddMonths(6) };
            meal.Instances = [.. guids.Select(guid => new MealInstance { MealInstanceId = guid, Meal = meal })];

            db.Meals.Add(meal);
            await db.SaveChangesAsync();

            Location? location = await db.Locations.FindAsync(locationId);
            await historyService.LogMealAddedAsync(name, quantity, location?.Name ?? "the freezer");
            return true;
        }

        /// <summary>
        /// Finds the meal instance for the given guid, with its batch and location loaded, if one exists.
        /// </summary>
        /// <param name="mealInstanceId">The guid printed on the instance's label.</param>
        public async Task<MealInstance?> FindInstanceAsync(Guid mealInstanceId)
        {
            return await db.MealInstances.Include(x => x.Meal).ThenInclude(x => x!.Location).FirstOrDefaultAsync(x => x.MealInstanceId == mealInstanceId);
        }

        /// <summary>
        /// Eats one meal instance, always consuming the whole thing. Deletes the parent batch too if this was its last remaining instance.
        /// </summary>
        /// <param name="mealInstanceId">The guid of the instance to eat.</param>
        public async Task EatAsync(Guid mealInstanceId)
        {
            MealInstance? instance = await db.MealInstances.Include(x => x.Meal).FirstOrDefaultAsync(x => x.MealInstanceId == mealInstanceId);
            if (instance is null)
            {
                return;
            }

            string mealName = instance.Meal!.Name;
            db.MealInstances.Remove(instance);

            bool hasOtherInstances = await db.MealInstances.AnyAsync(x => x.MealId == instance.MealId && x.MealInstanceId != mealInstanceId);
            if (!hasOtherInstances)
            {
                Meal? meal = await db.Meals.FirstOrDefaultAsync(x => x.MealId == instance.MealId);
                if (meal is not null)
                {
                    db.Meals.Remove(meal);
                }
            }

            await db.SaveChangesAsync();
            await historyService.LogMealEatenAsync(mealName);
        }

        /// <summary>
        /// Returns every meal batch with at least one remaining instance, with its location and instances loaded, for display alongside stock.
        /// </summary>
        public async Task<List<Meal>> GetAllWithInstancesAsync()
        {
            return await db.Meals.Include(x => x.Location).ThenInclude(x => x!.Category).Include(x => x.Instances).Where(x => x.Instances.Count > 0).ToListAsync();
        }
    }
}
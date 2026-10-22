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
        /// Finds the meal batch for the given ID, with its location, category, and instances loaded, if one exists.
        /// </summary>
        /// <param name="mealId">The ID of the batch.</param>
        public async Task<Meal?> FindAsync(int mealId)
        {
            return await db.Meals.Include(x => x.Location).ThenInclude(x => x!.Category).Include(x => x.Instances).FirstOrDefaultAsync(x => x.MealId == mealId);
        }

        /// <summary>
        /// Moves some or all of a batch's instances to a new location, preferring the given instance first. Moving the whole batch just relocates it; moving part of it merges the moved instances into a matching batch (same name and frozen date) at the destination, or splits them off into a new batch there.
        /// </summary>
        /// <param name="meal">The batch to move from, with its location and instances loaded.</param>
        /// <param name="locationId">The location to move to.</param>
        /// <param name="quantity">The number of instances to move.</param>
        /// <param name="preferredInstanceId">The instance to move first, such as the one that was scanned, if any.</param>
        public async Task MoveAsync(Meal meal, int locationId, int quantity, Guid? preferredInstanceId)
        {
            string fromLocationName = meal.Location?.Name ?? string.Empty;
            Location? toLocation = await db.Locations.FindAsync(locationId);
            List<MealInstance> toMove = [.. meal.Instances.OrderByDescending(x => x.MealInstanceId == preferredInstanceId).Take(quantity)];
            Meal? existing = await db.Meals.FirstOrDefaultAsync(x => x.MealId != meal.MealId && x.Name == meal.Name && x.FrozenAt == meal.FrozenAt && x.LocationId == locationId);

            if (existing is null && toMove.Count == meal.Instances.Count)
            {
                meal.LocationId = locationId;
            }
            else
            {
                Meal target = existing ?? new Meal { Name = meal.Name, LocationId = locationId, FrozenAt = meal.FrozenAt, Expiry = meal.Expiry };
                if (existing is null)
                {
                    db.Meals.Add(target);
                }

                foreach (MealInstance instance in toMove)
                {
                    instance.Meal = target;
                }

                if (toMove.Count == meal.Instances.Count)
                {
                    db.Meals.Remove(meal);
                }
            }

            await db.SaveChangesAsync();
            await historyService.LogMealMovedAsync(meal.Name, toMove.Count, fromLocationName, toLocation?.Name ?? string.Empty);
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
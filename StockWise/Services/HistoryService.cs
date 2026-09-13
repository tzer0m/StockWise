using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Builds and records human-readable history log messages for actions taken across the system.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class HistoryService(StockWiseDbContext db)
    {
        /// <summary>
        /// Logs that an item was added.
        /// </summary>
        /// <param name="name">The item's display name.</param>
        public Task LogItemAddedAsync(string name) => LogAsync($"{name} was added to items.");

        /// <summary>
        /// Logs that an item was updated.
        /// </summary>
        /// <param name="name">The item's display name.</param>
        public Task LogItemUpdatedAsync(string name) => LogAsync($"{name} was updated.");

        /// <summary>
        /// Logs that an item was deleted.
        /// </summary>
        /// <param name="name">The item's display name.</param>
        public Task LogItemDeletedAsync(string name) => LogAsync($"{name} was deleted from items.");

        /// <summary>
        /// Logs that a type was added.
        /// </summary>
        /// <param name="name">The type's display name.</param>
        public Task LogTypeAddedAsync(string name) => LogAsync($"{name} was added to types.");

        /// <summary>
        /// Logs that a type was renamed.
        /// </summary>
        /// <param name="name">The type's new display name.</param>
        public Task LogTypeUpdatedAsync(string name) => LogAsync($"{name} was updated.");

        /// <summary>
        /// Logs that a type was deleted.
        /// </summary>
        /// <param name="name">The type's display name.</param>
        public Task LogTypeDeletedAsync(string name) => LogAsync($"{name} was deleted from types.");

        /// <summary>
        /// Logs that a storage category was added.
        /// </summary>
        /// <param name="name">The category's display name.</param>
        public Task LogCategoryAddedAsync(string name) => LogAsync($"{name} was added to categories.");

        /// <summary>
        /// Logs that a storage category was updated.
        /// </summary>
        /// <param name="name">The category's new display name.</param>
        public Task LogCategoryUpdatedAsync(string name) => LogAsync($"{name} was updated.");

        /// <summary>
        /// Logs that a storage category was deleted.
        /// </summary>
        /// <param name="name">The category's display name.</param>
        public Task LogCategoryDeletedAsync(string name) => LogAsync($"{name} was deleted from categories.");

        /// <summary>
        /// Logs that a storage location was added.
        /// </summary>
        /// <param name="name">The location's display name.</param>
        public Task LogLocationAddedAsync(string name) => LogAsync($"{name} was added to locations.");

        /// <summary>
        /// Logs that a storage location was updated.
        /// </summary>
        /// <param name="name">The location's new display name.</param>
        public Task LogLocationUpdatedAsync(string name) => LogAsync($"{name} was updated.");

        /// <summary>
        /// Logs that a storage location was deleted.
        /// </summary>
        /// <param name="name">The location's display name.</param>
        public Task LogLocationDeletedAsync(string name) => LogAsync($"{name} was deleted from locations.");

        /// <summary>
        /// Logs that stock was added for an item.
        /// </summary>
        /// <param name="itemName">The item's display name.</param>
        /// <param name="quantity">The quantity added.</param>
        /// <param name="locationName">The location it was added at.</param>
        public Task LogStockAddedAsync(string itemName, int quantity, string locationName) => LogAsync($"{quantity} x {itemName} added to {locationName}.");

        /// <summary>
        /// Logs that stock was checked out for an item.
        /// </summary>
        /// <param name="itemName">The item's display name.</param>
        /// <param name="quantity">The quantity checked out.</param>
        /// <param name="locationName">The location it was checked out from.</param>
        public Task LogStockCheckedOutAsync(string itemName, int quantity, string locationName) => LogAsync($"{quantity} x {itemName} checked out from {locationName}.");

        /// <summary>
        /// Logs that a stock row was finished (removed entirely without a formal checkout).
        /// </summary>
        /// <param name="itemName">The item's display name.</param>
        /// <param name="locationName">The location it was finished at.</param>
        public Task LogStockFinishedAsync(string itemName, string locationName) => LogAsync($"{itemName} finished at {locationName}.");

        /// <summary>
        /// Logs that a unit of stock was opened and moved to a new location.
        /// </summary>
        /// <param name="itemName">The item's display name.</param>
        /// <param name="locationName">The location it was moved to.</param>
        public Task LogStockOpenedAsync(string itemName, string locationName) => LogAsync($"{itemName} opened, moved to {locationName}.");

        /// <summary>
        /// Logs that a frozen meal batch was added.
        /// </summary>
        /// <param name="name">The meal's display name.</param>
        /// <param name="quantity">The number of instances created.</param>
        /// <param name="locationName">The freezer location it was stored at.</param>
        public Task LogMealAddedAsync(string name, int quantity, string locationName) => LogAsync($"{quantity} x {name} added to {locationName}.");

        /// <summary>
        /// Logs that a meal instance was eaten.
        /// </summary>
        /// <param name="name">The meal's display name.</param>
        public Task LogMealEatenAsync(string name) => LogAsync($"{name} eaten.");

        /// <summary>
        /// Records a history message with the current UTC timestamp.
        /// </summary>
        /// <param name="message">The message to record.</param>
        private async Task LogAsync(string message)
        {
            db.History.Add(new History { Timestamp = DateTime.UtcNow, Message = message });
            await db.SaveChangesAsync();
        }
    }
}
namespace StockWise.Models
{
    /// <summary>
    /// Adapts a Meal batch (with its Location and Instances loaded) to the home page's combined inventory listing.
    /// </summary>
    /// <param name="Meal">The underlying meal batch.</param>
    public record MealRow(Meal Meal) : IInventoryRow
    {
        /// <summary>
        /// The meal's display name.
        /// </summary>
        public string Name => Meal.Name;

        /// <summary>
        /// Always "Meal", shown in place of a brand for a frozen meal batch.
        /// </summary>
        public string Brand => "Meal";

        /// <summary>
        /// The freezer location's display name.
        /// </summary>
        public string LocationName => Meal.Location?.Name ?? string.Empty;

        /// <summary>
        /// The freezer location's storage category colour.
        /// </summary>
        public string? CategoryColor => Meal.Location?.Category?.Color;

        /// <summary>
        /// The number of instances remaining in this batch.
        /// </summary>
        public int Quantity => Meal.Instances.Count;

        /// <summary>
        /// The batch's expiry date - always six months after it was frozen.
        /// </summary>
        public DateOnly? Expiry => Meal.Expiry;

        /// <summary>
        /// Always null - a meal batch is never "opened".
        /// </summary>
        public DateTime? OpenedAt => null;

        /// <summary>
        /// Always "Meals", used for the home page's type filter buttons.
        /// </summary>
        public string TypeName => "Meals";
    }
}
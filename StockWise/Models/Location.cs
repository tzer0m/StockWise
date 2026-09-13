namespace StockWise.Models
{
    /// <summary>
    /// A physical storage location within a category, e.g. a specific shelf or cupboard.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// The category this location belongs to.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The location's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The category this location belongs to.
        /// </summary>
        public StorageCategory? Category { get; set; }

        /// <summary>
        /// The stock currently held at this location.
        /// </summary>
        public List<Stock> Stock { get; set; } = [];

        /// <summary>
        /// The meal batches currently frozen at this location.
        /// </summary>
        public List<Meal> Meals { get; set; } = [];
    }
}
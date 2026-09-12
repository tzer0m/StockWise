namespace StockWise.Models
{
    /// <summary>
    /// A category used to group storage locations.
    /// </summary>
    public class StorageCategory
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The category's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The hex colour used to highlight this category's stock rows on the home page.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// The locations that belong to this category.
        /// </summary>
        public List<Location> Locations { get; set; } = [];

        /// <summary>
        /// The items allowed in this category, and under what conditions.
        /// </summary>
        public List<ItemStorageCategory> ItemStorageCategories { get; set; } = [];
    }
}
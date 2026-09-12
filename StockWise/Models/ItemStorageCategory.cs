namespace StockWise.Models
{
    /// <summary>
    /// Links an item to a storage category, and whether that category is valid unopened and/or opened.
    /// </summary>
    public class ItemStorageCategory
    {
        /// <summary>
        /// The linked item.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// The linked storage category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Whether this category is valid for the item while unopened.
        /// </summary>
        public bool AllowedWhenUnopened { get; set; }

        /// <summary>
        /// Whether this category is valid for the item once opened.
        /// </summary>
        public bool AllowedWhenOpened { get; set; }

        /// <summary>
        /// The linked item.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// The linked storage category.
        /// </summary>
        public StorageCategory? Category { get; set; }
    }
}
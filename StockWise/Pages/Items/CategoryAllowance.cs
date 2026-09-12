namespace StockWise.Pages.Items
{
    /// <summary>
    /// Whether a storage category is allowed for an item, unopened and/or opened.
    /// </summary>
    public class CategoryAllowance
    {
        /// <summary>
        /// The storage category's ID.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The storage category's display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this category is allowed for the item while unopened.
        /// </summary>
        public bool AllowedWhenUnopened { get; set; }

        /// <summary>
        /// Whether this category is allowed for the item once opened.
        /// </summary>
        public bool AllowedWhenOpened { get; set; }
    }
}
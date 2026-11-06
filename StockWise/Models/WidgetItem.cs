namespace StockWise.Models
{
    /// <summary>
    /// One expired or expiring item for the Tidgets widget.
    /// </summary>
    public class WidgetItem
    {
        /// <summary>
        /// The item's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Where the item is stored.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// The item's expiry date.
        /// </summary>
        public DateOnly Expiry { get; set; }
    }
}
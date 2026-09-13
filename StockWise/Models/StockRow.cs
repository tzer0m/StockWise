namespace StockWise.Models
{
    /// <summary>
    /// Adapts a Stock row (with its Item and Location loaded) to the home page's combined inventory listing.
    /// </summary>
    /// <param name="Stock">The underlying stock row.</param>
    public record StockRow(Stock Stock) : IInventoryRow
    {
        /// <summary>
        /// The stocked item's display name.
        /// </summary>
        public string Name => Stock.Item?.Name ?? string.Empty;

        /// <summary>
        /// The stocked item's brand, if known.
        /// </summary>
        public string Brand => Stock.Item?.Brand ?? string.Empty;

        /// <summary>
        /// The location's display name.
        /// </summary>
        public string LocationName => Stock.Location?.Name ?? string.Empty;

        /// <summary>
        /// The location's storage category colour.
        /// </summary>
        public string? CategoryColor => Stock.Location?.Category?.Color;

        /// <summary>
        /// The number of units held.
        /// </summary>
        public int Quantity => Stock.Quantity;

        /// <summary>
        /// The stock's expiry date, if any.
        /// </summary>
        public DateOnly? Expiry => Stock.Expiry;

        /// <summary>
        /// When this stock was opened, if it has been.
        /// </summary>
        public DateTime? OpenedAt => Stock.OpenedAt;

        /// <summary>
        /// The stocked item's type name, or empty if it hasn't been assigned one.
        /// </summary>
        public string TypeName => Stock.Item?.Type?.Name ?? string.Empty;
    }
}
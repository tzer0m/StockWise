namespace StockWise.Models
{
    /// <summary>
    /// A row in the home page's combined stock-and-meals listing - implemented by StockRow and MealRow so both can be merged into one list, sorted together, and rendered with per-type handling.
    /// </summary>
    public interface IInventoryRow
    {
        /// <summary>
        /// The display name shown in the Item column.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The display text shown in the Brand column - the item's real brand for stock, or "Meal" for a frozen meal batch.
        /// </summary>
        string Brand { get; }

        /// <summary>
        /// The location's display name.
        /// </summary>
        string LocationName { get; }

        /// <summary>
        /// The row's storage category colour, used for the row's left border and background tint, if known.
        /// </summary>
        string? CategoryColor { get; }

        /// <summary>
        /// The quantity held.
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// The expiry date, if any.
        /// </summary>
        DateOnly? Expiry { get; }

        /// <summary>
        /// When this row was opened, if applicable - always null for a meal row.
        /// </summary>
        DateTime? OpenedAt { get; }

        /// <summary>
        /// The row's type name, used for the home page's type filter buttons - the assigned item type for stock (empty if none is set), or "Meals" for a frozen meal batch.
        /// </summary>
        string TypeName { get; }
    }
}
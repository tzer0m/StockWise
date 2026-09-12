namespace StockWise.Models
{
    /// <summary>
    /// A quantity of an item held at a specific location.
    /// </summary>
    public class Stock
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// The item this stock is for.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// The location this stock is held at.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// The number of units held.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The expiry date, if the item has one.
        /// </summary>
        public DateOnly? Expiry { get; set; }

        /// <summary>
        /// When this stock row was added.
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// When this stock was opened, if it has been.
        /// </summary>
        public DateTime? OpenedAt { get; set; }

        /// <summary>
        /// The item this stock is for.
        /// </summary>
        public Item? Item { get; set; }

        /// <summary>
        /// The location this stock is held at.
        /// </summary>
        public Location? Location { get; set; }
    }
}
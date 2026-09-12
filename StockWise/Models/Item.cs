namespace StockWise.Models
{
    /// <summary>
    /// A trackable stock item, identified by barcode.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// The item's barcode.
        /// </summary>
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// The item's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A URL to an image of the item.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Whether a unit of this item can be opened, splitting it into a separate opened stock row.
        /// </summary>
        public bool IsOpenable { get; set; }

        /// <summary>
        /// How many days after opening this item expires, used to default the expiry date when it's opened.
        /// </summary>
        public int? ExpiryAfterOpeningDays { get; set; }

        /// <summary>
        /// When this item was first added to the system.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The storage categories this item is allowed in, and under what conditions.
        /// </summary>
        public List<ItemStorageCategory> ItemStorageCategories { get; set; } = [];

        /// <summary>
        /// The stock rows for this item.
        /// </summary>
        public List<Stock> Stock { get; set; } = [];
    }
}
namespace StockWise.Models
{
    /// <summary>
    /// An entry on the shopping list, either for an item type or a free-text name.
    /// </summary>
    public class ShoppingListItem
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int ShoppingListItemId { get; set; }

        /// <summary>
        /// The ID of the item type this entry is for, if it isn't free text.
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// The item type this entry is for, if it isn't free text.
        /// </summary>
        public ItemType? Type { get; set; }

        /// <summary>
        /// The free-text name, used when the entry isn't for an item type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Whether this entry was added automatically because the type's stock fell to its minimum quantity.
        /// </summary>
        public bool IsAutoAdded { get; set; }

        /// <summary>
        /// When this entry was added to the list.
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// The name to show on the list: the type's name, or the free-text name.
        /// </summary>
        public string DisplayName => Type?.Name ?? Name ?? string.Empty;
    }
}
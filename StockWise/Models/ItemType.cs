using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    /// <summary>
    /// A category of item, such as "Bread" or "Snack", used to power the home page's type filter buttons.
    /// </summary>
    public class ItemType
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// The type's display name.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The unopened stock level, across all items of this type, at or below which the type is added to the shopping list. Null means never.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Minimum quantity can't be negative.")]
        public int? MinQuantity { get; set; }

        /// <summary>
        /// The items assigned to this type.
        /// </summary>
        public List<Item> Items { get; set; } = [];
    }
}
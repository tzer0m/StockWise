using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    /// <summary>
    /// The fields for creating or editing an item, shared by every page that presents this form.
    /// </summary>
    public class ItemFormInput : IValidatableObject
    {
        /// <summary>
        /// The item's barcode.
        /// </summary>
        [Required(ErrorMessage = "Barcode is required.")]
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// The item's display name.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The item's brand, if known.
        /// </summary>
        [Required(ErrorMessage = "Brand is required.")]
        public string? Brand { get; set; }

        /// <summary>
        /// The ID of the item's type.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Select a type.")]
        public int TypeId { get; set; }

        /// <summary>
        /// A URL to an image of the item.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Whether a unit of this item can be opened.
        /// </summary>
        public bool IsOpenable { get; set; }

        /// <summary>
        /// How many days after opening this item expires, used to default the expiry date when it's opened.
        /// </summary>
        public int? ExpiryAfterOpeningDays { get; set; }

        /// <summary>
        /// The category allowances chosen for the item.
        /// </summary>
        public List<CategoryAllowance> CategoryAllowances { get; set; } = [];

        /// <summary>
        /// Validates the cross-field rules a single attribute can't express: an expiry-after-opening value when the item is openable, and at least one allowed category, unopened and (if openable) opened.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsOpenable && ExpiryAfterOpeningDays is null)
            {
                yield return new ValidationResult("Required when the item can be opened.", [nameof(ExpiryAfterOpeningDays)]);
            }

            if (!CategoryAllowances.Any(x => x.AllowedWhenUnopened))
            {
                yield return new ValidationResult("Select at least one category allowed when unopened.", [nameof(CategoryAllowances)]);
            }

            if (IsOpenable && !CategoryAllowances.Any(x => x.AllowedWhenOpened))
            {
                yield return new ValidationResult("Select at least one category allowed when opened.", [nameof(CategoryAllowances)]);
            }
        }
    }
}
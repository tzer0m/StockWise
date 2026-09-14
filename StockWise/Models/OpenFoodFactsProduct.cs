using System.Text.Json.Serialization;

namespace StockWise.Models
{
    /// <summary>
    /// The "product" object within an Open Food Facts API response.
    /// </summary>
    public class OpenFoodFactsProduct
    {
        /// <summary>
        /// The product's display name.
        /// </summary>
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        /// <summary>
        /// The product's brand(s), as a comma-separated string.
        /// </summary>
        [JsonPropertyName("brands")]
        public string? Brands { get; set; }
    }
}
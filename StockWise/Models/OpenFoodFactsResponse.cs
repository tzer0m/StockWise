using System.Text.Json.Serialization;

namespace StockWise.Models
{
    /// <summary>
    /// The top-level response from an Open Food Facts product lookup.
    /// </summary>
    public class OpenFoodFactsResponse
    {
        /// <summary>
        /// 1 if the product was found, 0 otherwise.
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// The matched product's details, if found.
        /// </summary>
        [JsonPropertyName("product")]
        public OpenFoodFactsProduct? Product { get; set; }
    }
}
namespace StockWise.Models
{
    /// <summary>
    /// A product's name and brand, looked up from Open Food Facts by barcode.
    /// </summary>
    public class OpenFoodFactsLookupResult
    {
        /// <summary>
        /// The product's display name, if known.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The product's brand, if known.
        /// </summary>
        public string? Brand { get; set; }
    }
}
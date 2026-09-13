namespace StockWise.Models
{
    /// <summary>
    /// A barcode and display name pair, used for client-side item matching while scanning a batch.
    /// </summary>
    public class BarcodeNameLookup
    {
        /// <summary>
        /// The item's barcode.
        /// </summary>
        public string Barcode { get; set; } = string.Empty;

        /// <summary>
        /// The item's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
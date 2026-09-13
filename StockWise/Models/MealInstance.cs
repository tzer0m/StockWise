namespace StockWise.Models
{
    /// <summary>
    /// One physical, individually-tagged frozen meal container - its guid is printed as a QR code on its label and scanned as its barcode.
    /// </summary>
    public class MealInstance
    {
        /// <summary>
        /// The primary key, and the guid printed as a QR code on this instance's label.
        /// </summary>
        public Guid MealInstanceId { get; set; }

        /// <summary>
        /// The batch this instance belongs to.
        /// </summary>
        public int MealId { get; set; }

        /// <summary>
        /// The batch this instance belongs to.
        /// </summary>
        public Meal? Meal { get; set; }
    }
}
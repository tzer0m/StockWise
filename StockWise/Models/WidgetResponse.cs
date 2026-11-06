namespace StockWise.Models
{
    /// <summary>
    /// The expired and expiring items shown by the Tidgets home screen widget.
    /// </summary>
    public class WidgetResponse
    {
        /// <summary>
        /// Items past their expiry date, oldest first.
        /// </summary>
        public List<WidgetItem> Expired { get; set; } = [];

        /// <summary>
        /// Items expiring within the warning window, soonest first.
        /// </summary>
        public List<WidgetItem> Expiring { get; set; } = [];
    }
}
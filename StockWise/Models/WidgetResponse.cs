namespace StockWise.Models
{
    /// <summary>
    /// The compact stock summary shown by the Tidgets home screen widget.
    /// </summary>
    public class WidgetResponse
    {
        /// <summary>
        /// The widget title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The main value, how many items are expired or expiring.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The supporting line, the next item to go off or how many more are expiring.
        /// </summary>
        public string Subtitle { get; set; } = string.Empty;
    }
}
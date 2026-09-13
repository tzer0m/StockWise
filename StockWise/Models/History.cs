using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    /// <summary>
    /// A single logged event describing an action taken elsewhere in the system.
    /// </summary>
    public class History
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// When the event happened, in UTC.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The human-readable description of what happened.
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
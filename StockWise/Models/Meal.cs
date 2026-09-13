namespace StockWise.Models
{
    /// <summary>
    /// A batch of home-made meals frozen together under one name and location, made up of one or more individually-tagged MealInstances.
    /// </summary>
    public class Meal
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int MealId { get; set; }

        /// <summary>
        /// The meal's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The freezer location this batch is stored at.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// The date this batch was frozen - always set to the date it was added, and never edited afterwards.
        /// </summary>
        public DateOnly FrozenAt { get; set; }

        /// <summary>
        /// The expiry date - always six months after FrozenAt.
        /// </summary>
        public DateOnly Expiry { get; set; }

        /// <summary>
        /// The freezer location this batch is stored at.
        /// </summary>
        public Location? Location { get; set; }

        /// <summary>
        /// The individually-tagged instances making up this batch.
        /// </summary>
        public List<MealInstance> Instances { get; set; } = [];
    }
}
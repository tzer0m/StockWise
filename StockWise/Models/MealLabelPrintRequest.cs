namespace StockWise.Models
{
    /// <summary>
    /// A request to tzer0mApi's Chitter print endpoint - one label per guid, all sharing the same heading text.
    /// </summary>
    /// <param name="Name">The heading text printed on every label.</param>
    /// <param name="Guids">The guids to encode, one physical label printed per entry.</param>
    public record MealLabelPrintRequest(string Name, List<Guid> Guids);
}
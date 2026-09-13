using System.Net.Http.Json;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Client for the tzer0mApi Chitter print endpoints used by the frozen-meals feature.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client, with BaseAddress and the API key header already set.</param>
    public class ChitterClient(HttpClient httpClient)
    {
        /// <summary>
        /// Sends one meal-label print job - the same heading text on every label, one label per guid.
        /// </summary>
        /// <param name="name">The heading text to print on every label.</param>
        /// <param name="guids">The guids to encode, one physical label per entry.</param>
        /// <returns>True if the printer accepted and printed the job, false otherwise.</returns>
        public async Task<bool> PrintMealLabelsAsync(string name, List<Guid> guids)
        {
            MealLabelPrintRequest request = new(name, guids);
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("Chitter/MealLabels", request);
            return response.IsSuccessStatusCode;
        }
    }
}
using System.Net.Http.Json;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Client for the Open Food Facts public barcode database.
    /// </summary>
    /// <param name="httpClient">The configured HTTP client, with BaseAddress and the User-Agent header already set.</param>
    public class OpenFoodFactsClient(HttpClient httpClient)
    {
        /// <summary>
        /// Looks up a product by barcode, returning its name and brand if Open Food Facts has a match.
        /// </summary>
        /// <param name="barcode">The barcode to look up.</param>
        public async Task<OpenFoodFactsLookupResult?> LookupAsync(string barcode)
        {
            OpenFoodFactsResponse? response;
            try
            {
                response = await httpClient.GetFromJsonAsync<OpenFoodFactsResponse>($"api/v2/product/{barcode}.json?fields=product_name,brands");
            }
            catch (HttpRequestException)
            {
                return null;
            }

            if (response?.Status != 1 || response.Product is null)
            {
                return null;
            }

            return new OpenFoodFactsLookupResult { Name = response.Product.ProductName, Brand = response.Product.Brands };
        }
    }
}
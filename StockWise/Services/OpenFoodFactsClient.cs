using System.Globalization;
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
        /// Looks up a product by barcode, returning its name and brand - cleaned up and title-cased - if Open Food Facts has a match.
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

            return new OpenFoodFactsLookupResult { Name = FormatText(response.Product.ProductName), Brand = FormatText(FirstBrand(response.Product.Brands)) };
        }

        /// <summary>
        /// Takes the first brand from Open Food Facts' comma-separated brands list, since it often includes a store's sub-brands or ranges alongside the main one.
        /// </summary>
        /// <param name="brands">The raw, comma-separated brands string.</param>
        private static string? FirstBrand(string? brands)
        {
            return string.IsNullOrWhiteSpace(brands) ? brands : brands.Split(',')[0];
        }

        /// <summary>
        /// Strips punctuation and title-cases text, since Open Food Facts data often comes back in inconsistent casing (e.g. all caps) with stray punctuation.
        /// </summary>
        /// <param name="text">The raw text to clean up.</param>
        private static string? FormatText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string withoutPunctuation = new([.. text.Where(x => !char.IsPunctuation(x))]);
            string collapsedWhitespace = string.Join(' ', withoutPunctuation.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(collapsedWhitespace.ToLowerInvariant());
        }
    }
}
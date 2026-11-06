using StockWise.Services;
using System.Security.Cryptography;
using System.Text;

namespace StockWise.Endpoints
{
    /// <summary>
    /// The API key protected endpoint that feeds the Tidgets home screen widget.
    /// </summary>
    public static class WidgetEndpoint
    {
        /// <summary>
        /// Returns the widget summary as JSON, or 401 if the API key is missing or wrong.
        /// </summary>
        /// <param name="request">The HTTP request, for the API key header.</param>
        /// <param name="widgetService">The widget service.</param>
        /// <param name="configuration">Used to read the expected API key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task<IResult> GetAsync(HttpRequest request, WidgetService widgetService, IConfiguration configuration, CancellationToken cancellationToken)
        {
            string? expected = configuration["Widget:ApiKey"];
            string provided = request.Headers["X-Api-Key"].ToString();
            if (string.IsNullOrEmpty(expected) || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected)))
            {
                return Results.Unauthorized();
            }
            return Results.Ok(await widgetService.GetAsync(cancellationToken));
        }
    }
}
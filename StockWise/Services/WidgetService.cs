using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Builds the compact expired and expiring summary shown by the Tidgets home screen widget.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="configuration">Used to read how many days ahead counts as expiring soon.</param>
    public class WidgetService(StockWiseDbContext db, IConfiguration configuration)
    {
        /// <summary>
        /// Counts stock and meals that have expired or expire soon, and picks out the next one to go off.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<WidgetResponse> GetAsync(CancellationToken cancellationToken)
        {
            int days = configuration.GetValue("Widget:ExpiringDays", 3);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly cutoff = today.AddDays(days);
            List<(string Name, DateOnly Expiry)> due = [];
            due.AddRange((await db.Stock.Include(x => x.Item).Where(x => x.Expiry != null && x.Expiry <= cutoff).ToListAsync(cancellationToken)).Select(x => (x.Item?.Name ?? string.Empty, x.Expiry!.Value)));
            due.AddRange((await db.Meals.Where(x => x.Expiry <= cutoff && x.Instances.Count > 0).ToListAsync(cancellationToken)).Select(x => (x.Name, x.Expiry)));
            int expired = due.Count(x => x.Expiry < today);
            List<(string Name, DateOnly Expiry)> expiring = [.. due.Where(x => x.Expiry >= today).OrderBy(x => x.Expiry)];
            string value = expired > 0 ? $"{expired} Expired" : expiring.Count > 0 ? $"{expiring.Count} Expiring" : "All Good";
            string subtitle;
            if (expired > 0)
            {
                subtitle = expiring.Count > 0 ? $"{expiring.Count} Expiring Soon" : "Nothing Else Due";
            }
            else if (expiring.Count > 0)
            {
                int offset = expiring[0].Expiry.DayNumber - today.DayNumber;
                subtitle = $"{expiring[0].Name} · {(offset == 0 ? "Today" : offset == 1 ? "Tomorrow" : $"In {offset} Days")}";
            }
            else
            {
                subtitle = $"Nothing Due in {days} Days";
            }
            return new WidgetResponse { Title = "StockWise", Value = value, Subtitle = subtitle };
        }
    }
}
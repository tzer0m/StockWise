using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;

namespace StockWise.Services
{
    /// <summary>
    /// Builds the expired and expiring lists shown by the Tidgets home screen widget.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="configuration">Used to read how many days ahead counts as expiring soon.</param>
    public class WidgetService(StockWiseDbContext db, IConfiguration configuration)
    {
        /// <summary>
        /// Gets the stock and meals that have expired or expire within the warning window.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<WidgetResponse> GetAsync(CancellationToken cancellationToken)
        {
            int days = configuration.GetValue("Widget:ExpiringDays", 1);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly cutoff = today.AddDays(days);
            List<WidgetItem> due = [];
            due.AddRange((await db.Stock.Include(x => x.Item).Include(x => x.Location).Where(x => x.Expiry != null && x.Expiry <= cutoff).ToListAsync(cancellationToken)).Select(x => new WidgetItem { Name = x.Item?.Name ?? string.Empty, Location = x.Location?.Name ?? string.Empty, Expiry = x.Expiry!.Value }));
            due.AddRange((await db.Meals.Include(x => x.Location).Where(x => x.Expiry <= cutoff && x.Instances.Count > 0).ToListAsync(cancellationToken)).Select(x => new WidgetItem { Name = x.Name, Location = x.Location?.Name ?? string.Empty, Expiry = x.Expiry }));
            return new WidgetResponse { Expired = [.. due.Where(x => x.Expiry < today).OrderBy(x => x.Expiry)], Expiring = [.. due.Where(x => x.Expiry >= today).OrderBy(x => x.Expiry)] };
        }
    }
}
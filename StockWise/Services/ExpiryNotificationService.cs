using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using t0m.Ting;

namespace StockWise.Services
{
    /// <summary>
    /// Background service that checks for stock nearing expiry and sends a daily digest notification via Ting.
    /// </summary>
    /// <param name="scopeFactory">Used to create a scope per run, since the database context is scoped.</param>
    /// <param name="tingClient">The Ting notification client.</param>
    /// <param name="configuration">Used to read the run time and warning window from appsettings.</param>
    public class ExpiryNotificationService(IServiceScopeFactory scopeFactory, TingClient tingClient, IConfiguration configuration) : BackgroundService
    {
        /// <summary>
        /// Runs the daily expiry check loop until the application shuts down.
        /// </summary>
        /// <param name="stoppingToken">Signals that the application is stopping.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan delay = GetDelayUntilNextRun();
                await Task.Delay(delay, stoppingToken);
                await CheckExpiringStockAsync(stoppingToken);
            }
        }

        /// <summary>
        /// Calculates how long to wait until the next scheduled run time.
        /// </summary>
        private TimeSpan GetDelayUntilNextRun()
        {
            int runAtHour = configuration.GetValue<int>("ExpiryNotifications:RunAtHour", 8);
            DateTime now = DateTime.Now;
            DateTime nextRun = new(now.Year, now.Month, now.Day, runAtHour, 0, 0);
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }

            return nextRun - now;
        }

        /// <summary>
        /// Finds stock expiring soon and sends a digest notification via Ting, one line per day.
        /// </summary>
        /// <param name="stoppingToken">Signals that the application is stopping.</param>
        private async Task CheckExpiringStockAsync(CancellationToken stoppingToken)
        {
            int daysAhead = configuration.GetValue<int>("ExpiryNotifications:DaysAhead", 1);
            using IServiceScope scope = scopeFactory.CreateScope();
            StockWiseDbContext db = scope.ServiceProvider.GetRequiredService<StockWiseDbContext>();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly cutoff = today.AddDays(daysAhead);
            List<Stock> expiring = await db.Stock.Include(x => x.Item).Include(x => x.Location).Where(x => x.Expiry != null && x.Expiry >= today && x.Expiry <= cutoff).OrderBy(x => x.Expiry).ToListAsync(stoppingToken);
            if (expiring.Count == 0)
            {
                return;
            }

            List<string> lines = [];
            for (int offset = 0; offset <= daysAhead; offset++)
            {
                DateOnly date = today.AddDays(offset);
                List<Stock> dayItems = expiring.Where(x => x.Expiry == date).ToList();
                if (dayItems.Count == 0)
                {
                    continue;
                }

                string label = offset switch { 0 => "Today", 1 => "Tomorrow", _ => date.ToString() };
                lines.Add($"{label}: {string.Join(", ", dayItems.Select(x => $"{x.Item?.Name} ({x.Location?.Name})"))}");
            }

            string body = string.Join("\n", lines);
            await tingClient.SendAsync("StockWise: items expiring soon", body);
        }
    }
}
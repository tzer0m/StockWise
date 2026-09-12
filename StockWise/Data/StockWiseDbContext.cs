using Microsoft.EntityFrameworkCore;

namespace StockWise.Data
{
    /// <summary>
    /// Database context for StockWise.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public class StockWiseDbContext(DbContextOptions<StockWiseDbContext> options) : DbContext(options)
    {
    }
}
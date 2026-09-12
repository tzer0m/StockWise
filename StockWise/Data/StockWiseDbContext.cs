using Microsoft.EntityFrameworkCore;
using StockWise.Models;

namespace StockWise.Data
{
    /// <summary>
    /// Database context for StockWise.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public class StockWiseDbContext(DbContextOptions<StockWiseDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Storage categories.
        /// </summary>
        public DbSet<StorageCategory> StorageCategories => Set<StorageCategory>();

        /// <summary>
        /// Storage locations.
        /// </summary>
        public DbSet<Location> Locations => Set<Location>();

        /// <summary>
        /// Trackable items.
        /// </summary>
        public DbSet<Item> Items => Set<Item>();

        /// <summary>
        /// Item-to-category links.
        /// </summary>
        public DbSet<ItemStorageCategory> ItemStorageCategories => Set<ItemStorageCategory>();

        /// <summary>
        /// Stock rows.
        /// </summary>
        public DbSet<Stock> Stock => Set<Stock>();

        /// <summary>
        /// Configures entity relationships and keys beyond what conventions infer.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ItemStorageCategories has a composite primary key rather than a single Id column.
            modelBuilder.Entity<ItemStorageCategory>().HasKey(x => new { x.ItemId, x.CategoryId });
        }
    }
}
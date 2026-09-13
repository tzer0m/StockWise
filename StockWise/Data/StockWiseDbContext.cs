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
        /// Item types.
        /// </summary>
        public DbSet<ItemType> Types => Set<ItemType>();

        /// <summary>
        /// Item-to-category links.
        /// </summary>
        public DbSet<ItemStorageCategory> ItemStorageCategories => Set<ItemStorageCategory>();

        /// <summary>
        /// Stock rows.
        /// </summary>
        public DbSet<Stock> Stock => Set<Stock>();

        /// <summary>
        /// Frozen meal batches.
        /// </summary>
        public DbSet<Meal> Meals => Set<Meal>();

        /// <summary>
        /// Individually-tagged meal instances.
        /// </summary>
        public DbSet<MealInstance> MealInstances => Set<MealInstance>();

        /// <summary>
        /// Logged history events.
        /// </summary>
        public DbSet<History> History => Set<History>();

        /// <summary>
        /// Configures entity relationships and keys beyond what conventions infer.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // StorageCategory's key is CategoryId, not the StorageCategoryId the key convention looks for.
            modelBuilder.Entity<StorageCategory>().HasKey(x => x.CategoryId);

            // ItemType's key is TypeId, not the ItemTypeId the key convention looks for.
            modelBuilder.Entity<ItemType>().HasKey(x => x.TypeId);

            // ItemStorageCategories has a composite primary key rather than a single Id column.
            modelBuilder.Entity<ItemStorageCategory>().HasKey(x => new { x.ItemId, x.CategoryId });

            // Add default storage categories.
            modelBuilder.Entity<StorageCategory>().HasData(
                new StorageCategory { CategoryId = 1, Name = "Fridge" },
                new StorageCategory { CategoryId = 2, Name = "Freezer" },
                new StorageCategory { CategoryId = 3, Name = "Pantry" });

            // Add default storage locations.
            modelBuilder.Entity<Location>().HasData(
                new Location { LocationId = 1, CategoryId = 1, Name = "Fridge Door" },
                new Location { LocationId = 2, CategoryId = 1, Name = "Fridge Shelves" },
                new Location { LocationId = 3, CategoryId = 2, Name = "Inside Freezer" },
                new Location { LocationId = 4, CategoryId = 2, Name = "Outside Freezer" },
                new Location { LocationId = 5, CategoryId = 3, Name = "Drawers" },
                new Location { LocationId = 6, CategoryId = 3, Name = "Cereal Cupboard" },
                new Location { LocationId = 7, CategoryId = 3, Name = "Cupboard L" },
                new Location { LocationId = 8, CategoryId = 3, Name = "Cupboard R" });
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Pantree.Server.Database.Configurations;
using Pantree.Server.Database.Entities.Cooking;

namespace Pantree.Server.Database
{
    /// <summary>
    /// The base <see cref="DbContext"/> for Pantree
    /// </summary>
    public class PantreeDataContext : DbContext
    {
        /// <summary>
        /// The <see cref="FoodEntity"/> models stored in the database
        /// </summary>
        public DbSet<FoodEntity> Foods { get; set; } = null!;

        /// <summary>
        /// The <see cref="IngredientEntity"/> models stored in the database
        /// </summary>
        public DbSet<IngredientEntity> Ingredients { get; set; } = null!;

        /// <summary>
        /// The <see cref="RecipeEntity"/> models stored in the database
        /// </summary>
        public DbSet<RecipeEntity> Recipes { get; set; } = null!;

        /// <summary>
        /// Construct a new <see cref="PantreeDataContext"/>
        /// </summary>
        /// <param name="options">A collection of configuration options to customize the database context</param>
        /// <returns>The new database context</returns>
        public PantreeDataContext(DbContextOptions<PantreeDataContext> options) : base(options) { }

        /// <summary>
        /// Construct a new <see cref="PantreeDataContext"/>
        /// </summary>
        /// <param name="options">A collection of configuration options to customize the database context</param>
        /// <returns>The new database context</returns>
        protected PantreeDataContext(DbContextOptions options) : base(options) { }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new FoodConfiguration());
            modelBuilder.ApplyConfiguration(new IngredientConfiguration());
            modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        }
    }
}

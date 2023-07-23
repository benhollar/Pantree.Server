using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pantree.Core.Cooking;
using Pantree.Server.Database.Entities.Cooking;

namespace Pantree.Server.Database.Configurations
{
    /// <summary>
    /// Configure the database mapping for <see cref="FoodEntity"/> models
    /// </summary>
    public class FoodConfiguration : IEntityTypeConfiguration<FoodEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<FoodEntity> builder)
        {
            builder.HasKey(food => food.Id);

            JsonSerializerOptions? jsonOptions = null;
            builder.Property(food => food.Nutrition)
                .HasConversion(
                    nutrition => JsonSerializer.Serialize(nutrition, jsonOptions),
                    json => JsonSerializer.Deserialize<Nutrition>(json, jsonOptions)
                );
            
            builder.OwnsOne(food => food.Measurement);

            builder.HasMany<IngredientEntity>(food => food.Ingredients)
                .WithOne(ingredient => ingredient.Food)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pantree.Server.Database.Entities.Cooking;

namespace Pantree.Server.Database.Configurations
{
    /// <summary>
    /// Configure the database mapping for <see cref="IngredientEntity"/> models
    /// </summary>
    public class IngredientConfiguration : IEntityTypeConfiguration<IngredientEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<IngredientEntity> builder)
        {
            builder.HasKey(ingredient => ingredient.Id);
            
            builder.OwnsOne(ingredient => ingredient.Quantity);
        }
    }
}

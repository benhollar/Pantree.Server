using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pantree.Server.Database.Entities.Cooking;

namespace Pantree.Server.Database.Configurations
{
    /// <summary>
    /// Configure the database mapping for <see cref="RecipeEntity"/> models
    /// </summary>
    public class RecipeConfiguration : IEntityTypeConfiguration<RecipeEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<RecipeEntity> builder)
        {
            builder.HasKey(recipe => recipe.Id);
            
            builder.HasMany(recipe => recipe.Ingredients).WithOne().OnDelete(DeleteBehavior.Cascade);

            JsonSerializerOptions? jsonOptions = null;
            builder.Property(recipe => recipe.Instructions)
                .HasConversion(
                    instructions => JsonSerializer.Serialize(instructions, jsonOptions),
                    json => JsonSerializer.Deserialize<List<string>>(json, jsonOptions)!,
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList())
                );
        }
    }
}

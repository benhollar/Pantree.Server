using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Measurement;
using Pantree.Server.Database;
using Pantree.Server.Database.Entities.Cooking;
using Pantree.Server.Models.Cooking;

namespace Pantree.Server.Controllers.Cooking
{
    /// <summary>
    /// An API controller for <see cref="Recipe"/> entities
    /// </summary>
    [ApiVersion("1.0")]
    public class RecipesController : BaseCollectionController<Recipe, RecipeEntity, RecipeDto>
    {
        /// <summary>
        /// Construct a new <see cref="RecipesController"/>
        /// </summary>
        /// <inheritdoc/>
        public RecipesController(PantreeDataContext context, IMapper mapper) : base(context, mapper) { }

        /// <inheritdoc/>
        protected override DbSet<RecipeEntity> Collection => _context.Recipes;

        /// <inheritdoc/>
        protected override async Task<List<RecipeEntity>> LoadAllAsync() => await Collection
            .IncludeAll()
            .ToListAsync();

        /// <inheritdoc/>
        protected override async Task<RecipeEntity> LoadSingleAsync(Guid id) => await Collection
            .Where(recipe => recipe.Id == id)
            .IncludeAll()
            .SingleAsync();

        /// <inheritdoc/>
        protected override async Task UpdateContext(RecipeEntity model)
        {
            foreach (IngredientEntity ingredient in model.Ingredients)
            {
                // If an ingredient appears in the list for the first time, it must be new; explicitly add it to the
                // database context
                if (await _context.Ingredients.SingleOrDefaultAsync(entity => entity.Id == ingredient.Id) is null)
                {
                    // Add the base ingredient
                    _context.Entry(ingredient).State = EntityState.Added;
                    // Add its quantity
                    EntityEntry<Measurement<FoodUnit>>? quantity = _context
                        .Entry(ingredient)
                        .Reference(x => x.Quantity).TargetEntry;
                    if (quantity is not null)
                        quantity.State = EntityState.Added;
                }

                // If the food referenced by the ingredient already existed -- which we generally expect -- we need to
                // explicitly replace the old food with the new one.
                FoodEntity food = ingredient.Food;
                if (await _context.Foods.SingleOrDefaultAsync(entity => entity.Id == food.Id) is FoodEntity existing)
                {
                    _context.Entry(existing).State = EntityState.Detached;
                    _context.Entry(food).State = EntityState.Modified;
                }
            }
        }
    }

    /// <summary>
    /// A collection of extension methods aimed to streamline working with <see cref="IQueryable"/> expressions for
    /// <see cref="RecipeEntity"/> values
    /// </summary>
    public static class RecipeQueryableExtensions
    {
        /// <summary>
        /// Fully load the entities in the provided <paramref name="queryable"/>
        /// </summary>
        /// <param name="queryable">The query expression to load data from</param>
        /// <returns>The adjusted query expression that will fully load the data referenced</returns>
        public static IQueryable<RecipeEntity> IncludeAll(this IQueryable<RecipeEntity> queryable) => queryable
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(ingredient => ingredient.Food).ThenInclude(food => food.Ingredients)
            .AsSplitQuery();
    }
}

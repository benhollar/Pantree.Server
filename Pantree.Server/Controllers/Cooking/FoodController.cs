using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pantree.Core.Cooking;
using Pantree.Server.Database;
using Pantree.Server.Database.Entities.Cooking;
using Pantree.Server.Models.Cooking;

namespace Pantree.Server.Controllers.Cooking
{
    /// <summary>
    /// An API controller for <see cref="Food"/> entities
    /// </summary>
    [ApiVersion("1.0")]
    public class FoodsController : BaseCollectionController<Food, FoodEntity, FoodDto>
    {
        /// <summary>
        /// Construct a new <see cref="FoodsController"/>
        /// </summary>
        /// <inheritdoc/>
        public FoodsController(PantreeDataContext context, IMapper mapper) : base(context, mapper) { }

        /// <inheritdoc/>
        protected override DbSet<FoodEntity> Collection => _context.Foods;

        /// <inheritdoc/>
        protected override async Task<List<FoodEntity>> LoadAllAsync() => await Collection.ToListAsync();

        /// <inheritdoc/>
        protected override async Task<FoodEntity> LoadSingleAsync(Guid id) => await Collection
            .Where(food => food.Id == id)
            .SingleAsync();

        /// <summary>
        /// Given an existing <see cref="Food"/> with a given <paramref name="id"/>, find all of the
        /// <see cref="Recipe"/> entries that use the specified food as an ingredient
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the food to query
        /// </param>
        /// <returns>
        /// An <see cref="OkObjectResult"/> with a body containing the recipe models, or any empty list if there are no
        /// matching recipes
        /// </returns>
        [HttpGet("{id}/recipes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecipesUsingFood(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not FoodEntity existing)
                return NotFound("An entity with the provided ID does not exist.");

            await Collection.Where(food => food.Id == guid)
                .Include(x => x.Ingredients)
                .SingleAsync();

            RecipeEntity[] allRecipes = await _context.Recipes.IncludeAll().ToArrayAsync();
            
            List<RecipeEntity> recipesUsingFood = new();
            foreach (IngredientEntity ingredient in existing.Ingredients)
                recipesUsingFood.AddRange(
                    allRecipes.Where(recipe => recipe.Ingredients.Any(x => x.Id == ingredient.Id))
                );

            return Ok(recipesUsingFood.Select(entity => _mapper.Map<RecipeDto>(_mapper.Map<Recipe>(entity))));
        }
    }
}

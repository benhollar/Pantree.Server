using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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
    public partial class RecipesController : BaseCollectionController<Recipe, RecipeEntity, RecipeDto>
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
                // explicitly tell EF that the existing food is being modified
                FoodEntity food = ingredient.Food;
                if (await _context.Foods.SingleOrDefaultAsync(entity => entity.Id == food.Id) is FoodEntity existing)
                {
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
        }
    }

    public partial class RecipesController
    {
        /// <summary>
        /// Retrieve the image associated with the recipe identified by <paramref name="id"/>
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the <see cref="Recipe"/> whose
        /// image should be retrieved
        /// </param>
        /// <returns>
        /// A <see cref="FileContentResult"/> (OK) containing the binary file data if it is found, and a
        /// <see cref="NoContentResult"/> if no image is stored for the recipe
        /// </returns>
        [HttpGet("{id}/image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImage(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not RecipeEntity existing)
                return NotFound("A recipe with the provided ID does not exist.");

            await LoadSingleAsync(existing);

            if (existing.ImageBlob is not null && existing.ImageContentType is not null)
                return new FileContentResult(existing.ImageBlob, existing.ImageContentType);
            else
                return NoContent();
        }

        /// <summary>
        /// Set the image representing the recipe identified by <paramref name="id"/>
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the <see cref="Recipe"/> whose
        /// image should be set
        /// </param>
        /// <param name="image">
        /// The file data, encoded in a multipart form containing an "image" field containing the image's binary data
        /// and a header specifying the content type
        /// </param>
        /// <returns>A <see cref="NoContentResult"/> if the image is successfully saved</returns>
        [HttpPost("{id}/image")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetImage(string id, IFormFile image)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not RecipeEntity existing)
                return NotFound("A recipe with the provided ID does not exist.");

            Task<RecipeEntity> recipeLoading = LoadSingleAsync(existing);

            using MemoryStream imageStream = new();
            Task imageCopying = image.CopyToAsync(imageStream);

            await Task.WhenAll(recipeLoading, imageCopying);

            existing.ImageBlob = imageStream.ToArray();
            existing.ImageContentType = image.ContentType;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete the image representing the recipe identified by <paramref name="id"/>
        /// </summary>
        /// <param name="id">
        /// The unique identifier, expected to be a valid <see cref="Guid"/>, identifying the <see cref="Recipe"/> whose
        /// image should be deleted
        /// </param>
        /// <returns>A <see cref="NoContentResult"/> if an image existed and was successfully deleted</returns>
        [HttpDelete("{id}/image")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteImage(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("The provided ID is not a valid GUID.");
            if (Collection.Where(model => model.Id == guid).SingleOrDefault() is not RecipeEntity existing)
                return NotFound("A recipe with the provided ID does not exist.");

            await LoadSingleAsync(existing);

            if (existing.ImageBlob is null && existing.ImageContentType is null)
                return BadRequest("The recipe does not have an image to delete.");
            
            existing.ImageBlob = null;
            existing.ImageContentType = null;

            await _context.SaveChangesAsync();
            return NoContent();
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

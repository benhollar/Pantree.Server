using System;
using System.Collections.Generic;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Interfaces;

namespace Pantree.Server.Database.Entities.Cooking
{
    /// <summary>
    /// A database-mapped model for a <see cref="Recipe"/>
    /// </summary>
    public class RecipeEntity : Identifiable
    {
        /// <inheritdoc/>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <inheritdoc cref="Recipe.Name"/>
        public string Name { get; set; } = "New Recipe";

        /// <inheritdoc cref="Recipe.Description"/>
        public string? Description { get; set; }

        /// <inheritdoc cref="Recipe.Instructions"/>
        public List<string> Instructions { get; set; } = new();

        /// <inheritdoc cref="Recipe.Ingredients"/>
        public List<IngredientEntity> Ingredients { get; set; } = new();

        /// <inheritdoc cref="Recipe.Servings"/>
        public uint Servings { get; set; } = 1;

        /// <inheritdoc cref="Recipe.PreparationTime"/>
        public TimeSpan? PreparationTime { get; set; }

        /// <inheritdoc cref="Recipe.CookingTime"/>
        public TimeSpan? CookingTime { get; set; }

        /// <summary>
        /// The binary data of the recipe's image, if one has been provided
        /// </summary>
        // TODO: eventually, I'd expect to replace this with reference to an external URL to find the image (e.g. S3),
        // as storing images in the database isn't a great long-term idea. But it'll suffice for initial development and
        // for a minimum-viable-product
        public byte[]? ImageBlob { get; set; }

        /// <summary>
        /// The content type of the recipe's image, if one exists
        /// </summary>
        public string? ImageContentType { get; set; }
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="RecipeEntity"/> to a <see cref="Recipe"/>, including the inverse
    /// mapping
    /// </summary>
    public class RecipeEntityProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="RecipeEntityProfile"/>
        /// </summary>
        public RecipeEntityProfile()
        {
            CreateMap<Recipe, RecipeEntity>()
                .ForMember(x => x.ImageContentType, opt => opt.Ignore())
                .ForMember(x => x.ImageBlob, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Interfaces;
using Pantree.Server.Utilities;

namespace Pantree.Server.Models.Cooking
{
    /// <summary>
    /// A DTO describing an <see cref="Ingredient"/>
    /// </summary>
    public record class IngredientDto : IdentifiableDto
    {
        /// <inheritdoc cref="Ingredient.Id"/>
        public string? Id { get; set; }

        /// <inheritdoc cref="Ingredient.Food"/>
        public FoodDto Food { get; set; }

        /// <inheritdoc cref="Ingredient.Quantity"/>
        public FoodMeasurementDto Quantity { get; set; }

        /// <inheritdoc cref="Ingredient.Nutrition"/>
        public Nutrition? Nutrition { get; set; }

        /// <summary>
        /// Construct a new <see cref="IngredientDto"/>
        /// </summary>
        /// <param name="food">The food component of the <see cref="IngredientDto"/></param>
        /// <param name="quantity">The necessary quantity of the <paramref name="food"/></param>
        public IngredientDto(FoodDto food, FoodMeasurementDto quantity)
        {
            Food = food;
            Quantity = quantity;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// An <see cref="IngredientDto"/> is valid when:
        /// <list type="number">
        /// <item>
        ///     Its ID, if specified, is a valid GUID
        /// </item>
        /// <item>
        ///     Its <see cref="Food"/> is valid as defined by <see cref="FoodDto.Validate(out List{string}?)"/>
        /// </item>
        /// <item>
        ///     Its <see cref="Quantity"/> is valid as defined by
        ///     <see cref="FoodMeasurementDto.Validate(out List{string}?)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public bool Validate([NotNullWhen(false)] out List<string>? errorMessages)
        {
            errorMessages = null;

            if (Id is not null && !Guid.TryParse(Id, out _))
                errorMessages = errorMessages.CreateOrAdd("The ID provided is not a valid GUID.");

            if (!Food.Validate(out List<string>? foodErrors))
                errorMessages = errorMessages.CreateOrAddRange(foodErrors);

            if (!Quantity.Validate(out List<string>? measurementErrors))
                errorMessages = errorMessages.CreateOrAddRange(measurementErrors);

            return errorMessages is null;
        }
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="IngredientDto"/> to a <see cref="Ingredient"/>, including the
    /// inverse mapping
    /// </summary>
    public class IngredientDtoProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="IngredientDtoProfile"/>
        /// </summary>
        public IngredientDtoProfile()
        {
            CreateMap<Ingredient, IngredientDto>()
                .ReverseMap();
        }
    }
}

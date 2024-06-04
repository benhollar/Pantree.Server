using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Interfaces;
using Pantree.Server.Utilities;

namespace Pantree.Server.Models.Cooking
{
    /// <summary>
    /// A DTO describing an <see cref="Recipe"/>
    /// </summary>
    public sealed record class RecipeDto : IdentifiableDto
    {
        /// <inheritdoc cref="Recipe.Id"/>
        public string? Id { get; set; }

        /// <inheritdoc cref="Recipe.Name"/>
        public string? Name { get; set; }

        /// <inheritdoc cref="Recipe.Description"/>
        public string? Description { get; set; }

        /// <inheritdoc cref="Recipe.Instructions"/>
        public List<string> Instructions { get; set; } = new();

        /// <inheritdoc cref="Recipe.Ingredients"/>
        public List<IngredientDto> Ingredients { get; set; } = new();

        /// <inheritdoc cref="Recipe.Servings"/>
        public uint? Servings { get; set; }

        /// <inheritdoc cref="Recipe.PreparationTime"/>
        /// <remarks>The time should be described as a number of minutes.</remarks>
        public uint? PreparationTime { get; set; }

        /// <inheritdoc cref="Recipe.CookingTime"/>
        /// <remarks>The time should be described as a number of minutes.</remarks>
        public uint? CookingTime { get; set; }

        /// <inheritdoc cref="Recipe.TotalTime"/>
        /// <remarks>The time should be described as a number of minutes.</remarks>
        public uint? TotalTime { get; set; }

        /// <inheritdoc cref="Recipe.TotalNutrition"/>
        public Nutrition? TotalNutrition { get; set; }

        /// <inheritdoc cref="Recipe.NutritionPerServing"/>
        public Nutrition? NutritionPerServing { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// A <see cref="RecipeDto"/> is valid when:
        /// <list type="number">
        ///     <item>Its ID, if specified, is a valid GUID</item>
        ///     <item>It has at least one instruction for preparing the recipe.</item>
        ///     <item>It has at least one ingredient.</item>
        ///     <item>
        ///     Every ingredient specified is valid as defined by
        ///     <see cref="IngredientDto.Validate(out List{string}?)"/>
        ///     </item>
        ///     <item>Its number of servings, if given, is nonnegative (i.e. greater than 0)</item>
        /// </list>
        /// </remarks>
        public bool Validate([NotNullWhen(false)] out List<string>? errorMessages)
        {
            errorMessages = null;

            if (Id is not null && !Guid.TryParse(Id, out _))
                errorMessages = errorMessages.CreateOrAdd("The ID provided is not a valid GUID.");

            if (Instructions.Count == 0)
                errorMessages = errorMessages
                    .CreateOrAdd("There must be at least one instruction for creating the recipe.");

            if (Ingredients.Count == 0)
                errorMessages = errorMessages.CreateOrAdd("There must be at least one ingredient for the recipe.");

            foreach (IngredientDto ingredient in Ingredients)
            {
                if (!ingredient.Validate(out List<string>? ingredientErrorMessages))
                {
                    ingredientErrorMessages = ingredientErrorMessages
                        .Select(message => $"({ingredient.Food.Name}): {message}")
                        .ToList();
                    errorMessages.CreateOrAddRange(ingredientErrorMessages);
                }    
            }

            if (Servings is not null && Servings == 0)
                errorMessages.CreateOrAdd("The recipe must make at least 1 serving.");

            return errorMessages is null;
        }

        /// <inheritdoc/>
        public bool Equals(RecipeDto? other)
        {
            if (other is null)
                return false;

            bool isEqual = true;
            isEqual = isEqual && Id == other.Id;
            isEqual = isEqual && Name == other.Name;
            isEqual = isEqual && Description == other.Description;
            isEqual = isEqual && Instructions.SequenceEqual(other.Instructions);
            isEqual = isEqual && Ingredients.SequenceEqualUnordered(other.Ingredients);
            isEqual = isEqual && Servings.Equals(other.Servings);
            isEqual = isEqual && PreparationTime.Equals(other.PreparationTime);
            isEqual = isEqual && CookingTime.Equals(other.CookingTime);
            isEqual = isEqual && TotalTime.Equals(other.TotalTime);
            isEqual = isEqual && TotalNutrition.Equals(other.TotalNutrition);
            isEqual = isEqual && NutritionPerServing.Equals(other.NutritionPerServing);

            return isEqual;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(Id);
            hashCode.Add(Name);
            hashCode.Add(Description);
            foreach (string instruction in Instructions)
                hashCode.Add(instruction);
            foreach (IngredientDto ingredient in Ingredients.OrderBy(x => x.Id))
                hashCode.Add(ingredient);
            hashCode.Add(Servings);
            hashCode.Add(PreparationTime);
            hashCode.Add(CookingTime);
            hashCode.Add(TotalTime);
            hashCode.Add(TotalNutrition);
            hashCode.Add(NutritionPerServing);
            return hashCode.ToHashCode();
        }
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="RecipeDto"/> to a <see cref="Recipe"/>, including the inverse
    /// mapping
    /// </summary>
    public class RecipeDtoProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="RecipeDtoProfile"/>
        /// </summary>
        public RecipeDtoProfile()
        {            
            CreateMap<Recipe, RecipeDto>()
                .ForMember(dto => dto.PreparationTime,
                           opt => opt.MapFrom(model => GetMinutesFromTimeSpan(model.PreparationTime)))
                .ForMember(dto => dto.CookingTime,
                           opt => opt.MapFrom(model => GetMinutesFromTimeSpan(model.CookingTime)))
                .ForMember(dto => dto.TotalTime,
                           opt => opt.MapFrom(model => GetMinutesFromTimeSpan(model.TotalTime)))
                .ReverseMap()
                    .ForMember(model => model.PreparationTime,
                               opt => opt.MapFrom(dto => GetTimeSpanFromMinutes(dto.PreparationTime)))
                    .ForMember(model => model.CookingTime,
                               opt => opt.MapFrom(dto => GetTimeSpanFromMinutes(dto.CookingTime)));
        }

        /// <summary>
        /// Get the number of minutes a <see cref="TimeSpan"/> spans, returning null if no timespan is given
        /// </summary>
        /// <param name="timespan">An optional timespan to get the minute duration of</param>
        /// <returns>The number of minutes, if given</returns>
        private uint? GetMinutesFromTimeSpan(TimeSpan? timespan) => (uint?)timespan?.TotalMinutes;

        /// <summary>
        /// Get a <see cref="TimeSpan"/> from a number of minutes, returning null if no minutes are given
        /// </summary>
        /// <param name="minutes">An optional amount of minutes describing a span of time</param>
        /// <returns>The <see cref="TimeSpan"/>, if given</returns>
        private TimeSpan? GetTimeSpanFromMinutes(uint? minutes) => minutes is not null
            ? TimeSpan.FromMinutes(minutes.Value)
            : null;
    }
}

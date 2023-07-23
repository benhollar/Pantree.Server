using System.Collections.Generic;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Measurement;

namespace Pantree.Server.Database.Entities.Cooking
{
    /// <summary>
    /// The database-mapped model for a <see cref="Food"/>
    /// </summary>
    public record class FoodEntity : Food
    {
        /// <summary>
        /// Construct a new <see cref="FoodEntity"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="Food"/></param>
        /// <returns></returns>
        public FoodEntity(string name) : base(name) { }

        /// <summary>
        /// Construct a new <see cref="FoodEntity"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="Food"/></param>
        /// <param name="baseNutrition">The base nutritional value of the <see cref="Food"/></param>
        /// <param name="baseMeasurement">The base serving size of the <see cref="Food"/></param>
        public FoodEntity(string name, Nutrition baseNutrition, Measurement<FoodUnit> baseMeasurement) 
            : base(name, baseNutrition, baseMeasurement) { }

        /// <summary>
        /// The <see cref="IngredientEntity"/> values referencing this <see cref="FoodEntity"/>
        /// </summary>
        /// <remarks>
        /// This value is intended to be managed by Entity Framework and not used directly.
        /// </remarks>
        public List<IngredientEntity> Ingredients { get; set; } = new();
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="FoodEntity"/> to a <see cref="Food"/>, including the inverse mapping
    /// </summary>
    public class FoodEntityProfile : Profile
    {
        /// <summary>
        /// Create a new <see cref="FoodEntityProfile"/>
        /// </summary>
        public FoodEntityProfile()
        {
            CreateMap<Food, FoodEntity>()
                .ForMember(entity => entity.Ingredients, opts => opts.Ignore())
                .ReverseMap();
        }
    }
}

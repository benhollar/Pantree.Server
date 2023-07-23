using System;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Interfaces;
using Pantree.Core.Utilities.Measurement;

namespace Pantree.Server.Database.Entities.Cooking
{
    /// <summary>
    /// The database-mapped model for a <see cref="Ingredient"/>
    /// </summary>
    public record class IngredientEntity : Identifiable
    {
        /// <inheritdoc/>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The <see cref="FoodEntity"/> referenced by this <see cref="IngredientEntity"/>
        /// </summary>
        public FoodEntity Food { get; set; }

        /// <summary>
        /// The quantity of the <see cref="Food"/> needed
        /// </summary>
        public Measurement<FoodUnit> Quantity { get; set; }

        /// <summary>
        /// Construct a new <see cref="IngredientEntity"/>
        /// </summary>
        /// <param name="food">The food used by this ingredient</param>
        /// <param name="quantity">The quantity of the food needed</param>
        public IngredientEntity(FoodEntity food, Measurement<FoodUnit> quantity)
        {
            Food = food;
            Quantity = quantity;
        }

        // Provided for Entity Framework compatibility, in which case nullability concerns are not valid as EF will
        // ensure the various properties are initialized after construction
        #nullable disable
        private IngredientEntity() { }
        #nullable enable
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="IngredientEntity"/> to a <see cref="Ingredient"/>, including the
    /// inverse mapping
    /// </summary>
    public class IngredientEntityProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="IngredientEntityProfile"/>
        /// </summary>
        public IngredientEntityProfile()
        {
            CreateMap<Ingredient, IngredientEntity>()
                .ReverseMap();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Measurement;
using Pantree.Server.Models.Interfaces;
using Pantree.Server.Utilities;

namespace Pantree.Server.Models.Cooking
{
    /// <summary>
    /// A DTO describing a <see cref="Measurement{FoodUnit}"/>
    /// </summary>
    public record class FoodMeasurementDto : IDto
    {
        /// <inheritdoc cref="Measurement{T}.Unit"/>
        public string Unit { get; set; } = FoodUnit.Unit.GetFriendlyName();

        /// <inheritdoc cref="Measurement{T}.Value"/>
        public double Value { get; set; } = 1;

        /// <inheritdoc/>
        /// <remarks>
        /// A <see cref="FoodMeasurementDto"/> is valid when:
        /// <list type="number">
        /// <item>
        ///     Its <see cref="Unit"/> corresponds to a valid <see cref="FoodUnit"/> option
        /// </item>
        /// <item>
        ///     Its <see cref="Value"/> is nonnegative (i.e. greater than 0)
        /// </item>
        /// </list>
        /// </remarks>
        public bool Validate([NotNullWhen(false)] out List<string>? errorMessages)
        {
            errorMessages = null;

            string[] unitNames = Enum.GetNames<FoodUnit>().Select(x => x.ToLowerInvariant()).ToArray();
            if (!unitNames.Contains(Unit.ToLowerInvariant()))
                errorMessages = errorMessages.CreateOrAdd(
                    $"The provided unit was not one of: {string.Join(", ", unitNames)}");
            
            if (Value <= 0)
                errorMessages = errorMessages.CreateOrAdd(
                    "The measurement's value must be strictly greater than 0.");

            return errorMessages is null;
        }
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="FoodMeasurementDto"/> to a <see cref="Measurement{T}"/>, including
    /// the inverse mapping
    /// </summary>
    public class FoodMeasurementDtoProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="FoodMeasurementDtoProfile"/>
        /// </summary>
        public FoodMeasurementDtoProfile()
        {
            CreateMap<Measurement<FoodUnit>, FoodMeasurementDto>()
                .ForMember(measurement => measurement.Unit,
                           opt => opt.MapFrom(measurement => measurement.Unit.GetFriendlyName()))
                .ReverseMap();
        }
    }

    /// <summary>
    /// A collection of extension methods for <see cref="FoodUnit"/>
    /// </summary>
    public static class FoodUnitExtensions
    {
        /// <summary>
        /// Get a user-friendly, display-ready name for a <see cref="FoodUnit"/> option
        /// </summary>
        /// <param name="unit">The unit to get the friendly name of</param>
        /// <returns>The friendly name</returns>
        public static string GetFriendlyName(this FoodUnit unit) => unit switch
        {
            FoodUnit.FluidOunce => "fluid ounce",
            _ => Enum.GetName<FoodUnit>(unit)!.ToLowerInvariant()
        };
    }
}

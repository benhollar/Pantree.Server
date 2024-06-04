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
    /// A DTO describing a <see cref="Food"/>
    /// </summary>
    public record class FoodDto : IdentifiableDto
    {
        /// <inheritdoc cref="Food.Id"/>
        public string? Id { get; set; }

        /// <inheritdoc cref="Food.Name"/>
        public string? Name { get; set; }

        /// <inheritdoc cref="Food.Nutrition"/>
        public Nutrition? Nutrition { get; set; }

        /// <inheritdoc cref="Food.Measurement"/>
        public FoodMeasurementDto? Measurement { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// A <see cref="FoodDto"/> is valid when:
        /// <list type="number">
        /// <item>
        ///     Its ID, if specified, is a valid GUID
        /// </item>
        /// <item>
        ///     Its <see cref="Measurement"/> is valid as defined by
        ///     <see cref="FoodMeasurementDto.Validate(out List{string}?)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public bool Validate([NotNullWhen(false)] out List<string>? errorMessages)
        {
            errorMessages = null;

            if (Id is not null && !Guid.TryParse(Id, out _))
                errorMessages = errorMessages.CreateOrAdd("The ID provided is not a valid GUID.");

            if (!Measurement.Validate(out List<string>? measurementErrors))
                errorMessages = errorMessages.CreateOrAddRange(measurementErrors);

            return errorMessages is null;
        }
    }

    /// <summary>
    /// A <see cref="Profile"/> to map a <see cref="FoodDto"/> to a <see cref="Food"/>, including the inverse mapping
    /// </summary>
    public class FoodDtoProfile : Profile
    {
        /// <summary>
        /// Construct a new <see cref="FoodDtoProfile"/>
        /// </summary>
        public FoodDtoProfile()
        {
            CreateMap<Food, FoodDto>()
                .ReverseMap();
        }
    }
}

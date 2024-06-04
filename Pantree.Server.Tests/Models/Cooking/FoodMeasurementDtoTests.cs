using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Measurement;
using Pantree.Server.Models.Cooking;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    public class FoodMeasurementDtoTests
    {
        private readonly IMapper _mapper;

        public FoodMeasurementDtoTests()
        {
            MapperConfiguration mapperConfig = new(cfg =>
            {
                cfg.AddMaps(Assembly.GetAssembly(typeof(FoodMeasurementDto)));
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Theory]
        [MemberData(nameof(ValidateTestData))]
        public void ValidateTest(FoodMeasurementDto dto, int expectedNumErrors)
        {
            bool valid = dto.Validate(out List<string>? errorMessages);
            if (expectedNumErrors == 0)
            {
                Assert.True(valid);
            }
            else
            {
                Assert.NotNull(errorMessages);
                Assert.Equal(expectedNumErrors, errorMessages.Count);
            }
        }

        [Theory]
        [MemberData(nameof(MapperTestData))]
        public void MapperTest(Measurement<FoodUnit> input, FoodMeasurementDto expected)
        {
            FoodMeasurementDto actual = _mapper.Map<FoodMeasurementDto>(input);
            Assert.Equal(expected, actual);

            Measurement<FoodUnit> reversed = _mapper.Map<Measurement<FoodUnit>>(actual);
            Assert.Equal(input, reversed);
        }

        public static IEnumerable<object?[]> ValidateTestData()
        {
            yield return new object?[]
            {
                new FoodMeasurementDto()
                {
                    Unit = "gram",
                    Value = 1
                },
                0
            };

            yield return new object?[]
            {
                new FoodMeasurementDto()
                {
                    Unit = "not a unit",
                    Value = 1
                },
                1
            };

            yield return new object?[]
            {
                new FoodMeasurementDto()
                {
                    Unit = "gram",
                    Value = -1
                },
                1
            };

            yield return new object?[]
            {
                new FoodMeasurementDto()
                {
                    Unit = "not a unit",
                    Value = -2
                },
                2
            };
        }

        public static IEnumerable<object?[]> MapperTestData()
        {
            yield return new object?[]
            {
                new Measurement<FoodUnit>(3.5, FoodUnit.Gram),
                new FoodMeasurementDto()
                {
                    Unit = "gram",
                    Value = 3.5
                }
            };

            yield return new object?[]
            {
                new Measurement<FoodUnit>(2, FoodUnit.FluidOunce),
                new FoodMeasurementDto()
                {
                    Unit = "fluid ounce",
                    Value = 2
                }
            };
        }
    }
}

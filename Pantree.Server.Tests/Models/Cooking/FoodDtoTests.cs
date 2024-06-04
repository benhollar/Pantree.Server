using System;
using System.Collections.Generic;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Cooking;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    public class FoodDtoTests
    {
        [Theory]
        [MemberData(nameof(ValidateTestData))]
        public void ValidateTest(FoodDto dto, int expectedNumErrors)
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

        public static IEnumerable<object?[]> ValidateTestData()
        {
            yield return new object?[]
            {
                new FoodDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test Food",
                    Nutrition = null,
                    Measurement = new FoodMeasurementDto()
                    {
                        Unit = "gram",
                        Value = 1
                    }
                },
                0
            };

            yield return new object?[]
            {
                new FoodDto()
                {
                    Id = "not a guid",
                    Name = "Test Food",
                    Nutrition = new Nutrition() { Calories = 100 },
                    Measurement = new FoodMeasurementDto()
                    {
                        Unit = "gram",
                        Value = 1
                    }
                },
                1
            };

            yield return new object?[]
            {
                new FoodDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test Food",
                    Nutrition = null,
                    Measurement = new FoodMeasurementDto()
                    {
                        Unit = "not a unit",
                        Value = 1
                    }
                },
                1
            };

            yield return new object?[]
            {
                new FoodDto()
                {
                    Id = "not a guid",
                    Name = "Test Food",
                    Nutrition = null,
                    Measurement = new FoodMeasurementDto()
                    {
                        Unit = "not a unit",
                        Value = -1
                    }
                },
                3
            };
        }
    }
}

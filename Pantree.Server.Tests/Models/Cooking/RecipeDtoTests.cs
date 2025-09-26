using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Cooking;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    public class RecipeDtoTests
    {
        private readonly IMapper _mapper;

        public RecipeDtoTests()
        {
            MapperConfiguration mapperConfig = new(cfg =>
            {
                cfg.AddMaps(Assembly.GetAssembly(typeof(RecipeDto)));
            }, new LoggerFactory());
            _mapper = mapperConfig.CreateMapper();
        }

        [Theory]
        [MemberData(nameof(ValidateTestData))]
        public void ValidateTest(RecipeDto dto, int expectedNumErrors)
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
        [MemberData(nameof(EqualsTestData))]
        public void EqualsTest(RecipeDto lhs, RecipeDto? rhs, bool expected)
        {
            bool equal = lhs.Equals(rhs);
            Assert.Equal(expected, equal);

            // Further, evaluate the custom `GetHashCode` adheres to the expected properties
            int lhsHash = lhs.GetHashCode();
            int? rhsHash = rhs?.GetHashCode();
            //  Equal objects have equal hashes
            if (expected)
                Assert.Equal(lhsHash, rhsHash);
            //  Repeated calls to `GetHashCode` return the same result if the object is unmodified
            Assert.Equal(lhsHash, lhs.GetHashCode());
            //  And likewise, modifying the object modifies its hash code
            string? originalName = lhs.Name;
            lhs.Name = "A new name";
            Assert.NotEqual(lhsHash, lhs.GetHashCode());
            lhs.Name = originalName;
        }

        [Theory]
        [MemberData(nameof(MapperTestData))]
        public void MapperTest(Recipe input, RecipeDto expected)
        {
            RecipeDto actual = _mapper.Map<RecipeDto>(input);
            Assert.Equal(expected, actual);

            Recipe reversed = _mapper.Map<Recipe>(actual);
            Assert.Equal(input, reversed);
        }

        public static IEnumerable<object?[]> ValidateTestData()
        {
            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new() { "1", "2", "3"},
                    Ingredients = new()
                    {
                        new IngredientDto(
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
                            new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                            
                        )
                        {
                            Id = Guid.NewGuid().ToString()
                        },
                    },
                    Servings = 1,
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    TotalNutrition = new() { Calories = 100 },
                    NutritionPerServing = new() { Calories = 100 }
                },
                0   
            };

            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = "not a guid",
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new() { "1", "2", "3"},
                    Ingredients = new()
                    {
                        new IngredientDto(
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
                            new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                            
                        )
                        {
                            Id = Guid.NewGuid().ToString()
                        },
                    },
                    Servings = 1,
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    TotalNutrition = new() { Calories = 100 },
                    NutritionPerServing = new() { Calories = 100 }
                },
                1  
            };

            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = "not a guid",
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new(),
                    Ingredients = new()
                    {
                        new IngredientDto(
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
                            new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                            
                        )
                        {
                            Id = Guid.NewGuid().ToString()
                        },
                    },
                    Servings = 1,
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    TotalNutrition = new() { Calories = 100 },
                    NutritionPerServing = new() { Calories = 100 }
                },
                2, 
            };

            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = "not a guid",
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new(),
                    Ingredients = new(),
                    Servings = 1,
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    TotalNutrition = new() { Calories = 100 },
                    NutritionPerServing = new() { Calories = 100 }
                },
                3, 
            };

            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = "not a guid",
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new(),
                    Ingredients = new(),
                    Servings = 0,
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    TotalNutrition = new() { Calories = 100 },
                    NutritionPerServing = new() { Calories = 100 }
                },
                4, 
            };

            yield return new object?[]
            {
                new RecipeDto()
                {
                    Id = "not a guid",
                    Name = "Test Recipe",
                    Description = "Lorem ipsum...",
                    Instructions = new(),
                    Ingredients = new()
                    {
                        new IngredientDto(
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
                            new FoodMeasurementDto()
                            {
                                Unit = "not a unit",
                                Value = -1
                            }
                            
                        )
                        {
                            Id = "not a guid"
                        },
                    },
                    Servings = null,
                    PreparationTime = null,
                    CookingTime = null,
                    TotalTime = null,
                    TotalNutrition = null,
                    NutritionPerServing = null,
                },
                8, 
            };
        }

        public static IEnumerable<object?[]> EqualsTestData()
        {
            RecipeDto example1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Recipe",
                Description = "Lorem ipsum...",
                Instructions = new() { "1", "2", "3" },
                Ingredients = new()
                {
                    new IngredientDto(
                        new FoodDto()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Test Food 1",
                            Nutrition = null,
                            Measurement = new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                        },
                        new FoodMeasurementDto()
                        {
                            Unit = "gram",
                            Value = 1
                        }
                        
                    )
                    {
                        Id = Guid.NewGuid().ToString()
                    },
                    new IngredientDto(
                        new FoodDto()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Test Food 2",
                            Nutrition = null,
                            Measurement = new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                        },
                        new FoodMeasurementDto()
                        {
                            Unit = "gram",
                            Value = 1
                        }
                    )
                    {
                        Id = Guid.NewGuid().ToString()
                    },
                },
                Servings = 1,
                PreparationTime = 30,
                CookingTime = 40,
                TotalTime = 70,
                TotalNutrition = new() { Calories = 100 },
                NutritionPerServing = new() { Calories = 100 }
            };
            
            // example1 with reordered ingredients
            RecipeDto example2 = new()
            {
                Id = example1.Id,
                Name = "Test Recipe",
                Description = "Lorem ipsum...",
                Instructions = new() { "1", "2", "3" },
                Ingredients = new()
                {
                    new IngredientDto(
                        new FoodDto()
                        {
                            Id = example1.Ingredients[1].Food.Id,
                            Name = "Test Food 2",
                            Nutrition = null,
                            Measurement = new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                        },
                        new FoodMeasurementDto()
                        {
                            Unit = "gram",
                            Value = 1
                        }
                    )
                    {
                        Id = example1.Ingredients[1].Id
                    },
                    new IngredientDto(
                        new FoodDto()
                        {
                            Id = example1.Ingredients[0].Food.Id,
                            Name = "Test Food 1",
                            Nutrition = null,
                            Measurement = new FoodMeasurementDto()
                            {
                                Unit = "gram",
                                Value = 1
                            }
                        },
                        new FoodMeasurementDto()
                        {
                            Unit = "gram",
                            Value = 1
                        }
                        
                    )
                    {
                        Id = example1.Ingredients[0].Id
                    },
                },
                Servings = 1,
                PreparationTime = 30,
                CookingTime = 40,
                TotalTime = 70,
                TotalNutrition = new() { Calories = 100 },
                NutritionPerServing = new() { Calories = 100 }
            };

            RecipeDto example3 = new();

            yield return new object?[] { example1, example1, true };
            yield return new object?[] { example1, example2, true };
            yield return new object?[] { example1, example3, false };
            yield return new object?[] { example1, null, false };
            yield return new object?[] { example3, null, false };
        }

        public static IEnumerable<object?[]> MapperTestData()
        {
            yield return new object?[]
            {
                new Recipe()
                {
                    Id = Guid.Empty,
                    Name = "Test",
                    Description = "Lorem ipsum...",
                    Ingredients = new()
                    {
                        new Ingredient(new Food("Test Food 1") { Id = Guid.Empty }, new(1, FoodUnit.Gram))
                        {
                            Id = Guid.Empty
                        },
                        new Ingredient(new Food("Test Food 2") { Id = Guid.Empty }, new(2, FoodUnit.FluidOunce))
                        {
                            Id = Guid.Empty
                        }
                    },
                    Instructions = new() { "1", "2", "3" },
                    PreparationTime = TimeSpan.FromMinutes(30),
                    CookingTime = TimeSpan.FromMinutes(40),
                    Servings = 1
                },
                new RecipeDto()
                {
                    Id = Guid.Empty.ToString(),
                    Name = "Test",
                    Description = "Lorem ipsum...",
                    Ingredients = new()
                    {
                        new IngredientDto(
                            new FoodDto() { Id = Guid.Empty.ToString(), Name = "Test Food 1" },
                            new FoodMeasurementDto() { Value = 1, Unit = "gram" }
                        )
                        {
                            Id = Guid.Empty.ToString()
                        },
                        new IngredientDto(
                            new FoodDto() { Id = Guid.Empty.ToString(), Name = "Test Food 2" },
                            new FoodMeasurementDto() { Value = 2, Unit = "fluid ounce" }
                        )
                        {
                            Id = Guid.Empty.ToString()
                        },
                    },
                    Instructions = new() { "1", "2", "3" },
                    PreparationTime = 30,
                    CookingTime = 40,
                    TotalTime = 70,
                    Servings = 1
                }
            };
        }
    }
}

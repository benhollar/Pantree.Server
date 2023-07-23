using System;
using System.Collections.Generic;
using Pantree.Core.Cooking;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    internal static class CookingDataStubs
    {
        internal static List<Food> ThreeDummyFoods()
        {
            return new()
            {
                new Food("Test Food 1")
                {
                    Id = Guid.NewGuid(),
                    Nutrition = new()
                    {
                        Calories = 100
                    },
                    Measurement = new(10, FoodUnit.Milligram)
                },
                new Food("Test Food 2")
                {
                    Id = Guid.NewGuid(),
                    Nutrition = new()
                    {
                        Calories = 200
                    },
                    Measurement = new(20, FoodUnit.Liter)
                },
                new Food("Test Food 3")
                {
                    Id = Guid.NewGuid(),
                    Nutrition = new()
                    {
                        Calories = 300
                    },
                    Measurement = new(30, FoodUnit.Ounce)
                }
            };
        }

        internal static List<Recipe> ThreeDummyRecipes()
        {
            List<Food> foods = ThreeDummyFoods();

            return new()
            {
                new Recipe()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Recipe 1",
                    Description = "The first dummy recipe.",
                    Instructions = new()
                    {
                        "Test Recipe 1, Instruction 1",
                        "Test Recipe 1, Instruction 2",
                        "Test Recipe 1, Instruction 3"
                    },
                    Ingredients = new()
                    {
                        new Ingredient(foods[0], new(1, FoodUnit.Gram)),
                        new Ingredient(foods[2], new(20, FoodUnit.Milliliter))
                    },
                    Servings = 4,
                    PreparationTime = TimeSpan.FromMinutes(20),
                    CookingTime = TimeSpan.FromMinutes(30)
                },
                new Recipe()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Recipe 2",
                    Description = "The second dummy recipe.",
                    Instructions = new()
                    {
                        "Test Recipe 2, Instruction 1",
                        "Test Recipe 2, Instruction 2",
                        "Test Recipe 2, Instruction 3"
                    },
                    Ingredients = new()
                    {
                        new Ingredient(foods[1], new(5, FoodUnit.Pound)),
                        new Ingredient(foods[2], new(3, FoodUnit.Ounce))
                    },
                    Servings = 2,
                    PreparationTime = TimeSpan.FromMinutes(10),
                    CookingTime = TimeSpan.FromMinutes(20)
                },
                new Recipe()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Recipe 3",
                    Description = "The third dummy recipe.",
                    Instructions = new()
                    {
                        "Test Recipe 3, Instruction 1",
                        "Test Recipe 3, Instruction 2",
                        "Test Recipe 3, Instruction 3"
                    },
                    Ingredients = new()
                    {
                        new Ingredient(foods[0], new(7, FoodUnit.FluidOunce)),
                        new Ingredient(foods[2], new(11, FoodUnit.Teaspoon))
                    },
                    Servings = 6,
                    PreparationTime = TimeSpan.FromMinutes(5),
                    CookingTime = TimeSpan.FromMinutes(10)
                },
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Cooking;
using Pantree.Server.Tests.Utilities;
using Pantree.Server.Tests.Utilities.Database;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    public class FoodControllerTests
    {
        private readonly HttpClient _client;
        private readonly PantreeWebApplicationFactory _factory;

        public FoodControllerTests()
        {
            _factory = new();
            _client = _factory.CreateClient();
        }

        [Theory]
        [MemberData(nameof(GetAllTestData))]
        public async void GetAllTest(List<Food> seededFoods, List<FoodDto> expectedFoods)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);

            // Make the basic request; we're expecting an "OK" response at all times
            HttpResponseMessage response = await _client.GetAsync("/api/foods/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Check the validity of the response
            List<FoodDto>? actualFoods = await response.Content.DeserializeRequestContent<List<FoodDto>>();
            CustomAssertions.EqualUnordered(expectedFoods, actualFoods);
        }

        [Theory]
        [MemberData(nameof(GetSingleTestData))]
        public async void GetSingleTest(List<Food> seededFoods, Guid id, FoodDto? expectedFood)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);

            // Make the request; depending on the test data, we expect either an error response or matching DTO
            HttpResponseMessage response = await _client.GetAsync($"/api/foods/{id}");
            if (expectedFood is null) 
            {
                // Error is expected; validate the API handles that correctly with a NotFound
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            else
            {
                // Matching DTO is expected
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                FoodDto? actualFood = await response.Content.DeserializeRequestContent<FoodDto>();
                Assert.Equal(expectedFood, actualFood);
            }
        }

        [Theory]
        [MemberData(nameof(AddOrEditTestData))]
        public async void AddOrEditTest(List<Food> seededFoods, FoodDto newModel)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);

            // Make the request; the correct response depends on if the entity being "added" already existed or not
            HttpResponseMessage response = await _client.PostAsync("/api/foods/", 
                JsonContent.Create<FoodDto>(newModel));
            FoodDto? actualFood = await response.Content.DeserializeRequestContent<FoodDto>();

            bool alreadyExists = seededFoods.SingleOrDefault(food => food.Id.ToString() == newModel.Id) is not null;
            if (alreadyExists)
            {
                // The user used the POST endpoint to edit an item
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(newModel, actualFood);
            }
            else
            {
                // The user added a new item
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(response.Headers.Contains("location"));
                Assert.Equal(response.Headers.GetValues("location").Single(), actualFood!.Id);

                newModel.Id = actualFood.Id;
                Assert.Equal(newModel, actualFood);
            }
        }

        [Theory]
        [MemberData(nameof(AddOrEditTestData))]
        public async void EditTest(List<Food> seededFoods, FoodDto changedModel)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);

            // Make the request; the correct response depends on if the entity being edited exists or not
            Assert.NotNull(changedModel.Id);
            HttpResponseMessage response = await _client.PutAsync($"/api/foods/{changedModel.Id}", 
                JsonContent.Create<FoodDto>(changedModel));

            // To get a successful response, the ID must already exist. If it doesn't, we expect a NotFound
            bool alreadyExists = seededFoods.SingleOrDefault(food => food.Id.ToString() == changedModel.Id) is not null;
            if (alreadyExists)
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(DeleteTestData))]
        public async void DeleteTest(List<Food> seededFoods, string id)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);

            // Make the request; the correct response depends on if the entity being deleted already existed or not
            HttpResponseMessage response = await _client.DeleteAsync($"/api/foods/{id}");

            // If the entity exists, we expect a successful, empty status code; otherwise we expect NotFound
            bool alreadyExists = seededFoods.SingleOrDefault(food => food.Id.ToString() == id) is not null;
            if (alreadyExists)
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(RecipesUsingFoodTestData))]
        public async void RecipesUsingFoodTest(
            List<Food> seededFoods,
            List<Recipe> seededRecipes,
            string foodId,
            List<Recipe> expectedResult)
        {
            // Seed the database
            SeedData.SeedFoods(_factory, seededFoods);
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the basic request; we're expecting an "OK" response at all times
            HttpResponseMessage response = await _client.GetAsync($"/api/foods/{foodId}/recipes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Check the validity of the response
            List<RecipeDto>? actualRecipes = await response.Content.DeserializeRequestContent<List<RecipeDto>>();
            CustomAssertions.EqualUnordered(
                expectedResult.Select(x => x.Id.ToString()),
                actualRecipes?.Select(x => x.Id)
            );
        }

        public static IEnumerable<object?[]> GetAllTestData()
        {
            List<Guid> guids = Enumerable.Range(0, 3).Select(x => Guid.NewGuid()).ToList();
            List<string> names = Enumerable.Range(0, 3).Select(x => $"Test Food {x}").ToList();

            yield return new object[]
            {
                new List<Food>() { },
                new List<FoodDto>() { }
            };

            yield return new object[]
            {
                new List<Food>()
                {
                    new Food(names[0])
                    {
                        Id = guids[0],
                        Nutrition = new()
                        {
                            Calories = 100
                        },
                        Measurement = new(1, FoodUnit.Tablespoon)
                    }
                },
                new List<FoodDto>()
                {
                    new FoodDto()
                    {
                        Id = guids[0].ToString(),
                        Name = names[0],
                        Nutrition = new()
                        {
                            Calories = 100
                        },
                        Measurement = new() { Unit = "tablespoon", Value = 1 }
                    }
                }
            };

            yield return new object[]
            {
                new List<Food>()
                {
                    new Food(names[1])
                    {
                        Id = guids[1],
                        Nutrition = new()
                        {
                            Calories = 200
                        },
                        Measurement = new(2, FoodUnit.Cup)
                    },
                    new Food(names[2])
                    {
                        Id = guids[2],
                        Nutrition = new()
                        {
                            Calories = 300
                        },
                        Measurement = new(3, FoodUnit.FluidOunce)
                    }
                },
                new List<FoodDto>()
                {
                    new FoodDto()
                    {
                        Id = guids[1].ToString(),
                        Name = names[1],
                        Nutrition = new()
                        {
                            Calories = 200
                        },
                        Measurement = new() { Unit = "cup", Value = 2 }
                    },
                    new FoodDto()
                    {
                        Id = guids[2].ToString(),
                        Name = names[2],
                        Nutrition = new()
                        {
                            Calories = 300
                        },
                        Measurement = new() { Unit = "fluid ounce", Value = 3 }
                    }
                }
            };
        }
    
        public static IEnumerable<object?[]> GetSingleTestData()
        {
            List<Food> foods = CookingDataStubs.ThreeDummyFoods();

            yield return new object[]
            {
                foods,
                foods[0].Id,
                new FoodDto()
                {
                    Id = foods[0].Id.ToString(),
                    Name = "Test Food 1",
                    Nutrition = new()
                    {
                        Calories = 100
                    },
                    Measurement = new() { Unit = "milligram", Value = 10 }
                }
            };
            yield return new object?[]
            {
                foods,
                Guid.NewGuid(),
                null
            };
            yield return new object[]
            {
                foods,
                foods[2].Id,
                new FoodDto()
                {
                    Id = foods[2].Id.ToString(),
                    Name = "Test Food 3",
                    Nutrition = new()
                    {
                        Calories = 300
                    },
                    Measurement = new() { Unit = "ounce", Value = 30 }
                }
            };
        }

        public static IEnumerable<object?[]> AddOrEditTestData()
        {
            List<Food> foods = CookingDataStubs.ThreeDummyFoods();

            yield return new object[] // add a new food
            {
                foods,
                new FoodDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Not a dummy food",
                    Nutrition = new()
                    {
                        Calories = 100
                    },
                    Measurement = new() { Unit = "tablespoon", Value = 2 }
                }
            };
            yield return new object[] // edit an existing food
            {
                foods,
                new FoodDto()
                {
                    Id = foods[0].Id.ToString(),
                    Name = "A dummy food that got edited",
                    Nutrition = new()
                    {
                        Calories = 1000
                    },
                    Measurement = new() { Unit = "teaspoon", Value = 30 }
                }
            };
        }

        public static IEnumerable<object?[]> DeleteTestData()
        {
            List<Food> foods = CookingDataStubs.ThreeDummyFoods();

            yield return new object[] { foods, foods[0].Id.ToString() };
            yield return new object[] { foods, foods[2].Id.ToString() };
            yield return new object[] { foods, Guid.NewGuid().ToString() };
        }

        public static IEnumerable<object[]> RecipesUsingFoodTestData()
        {
            List<Recipe> seededRecipes = CookingDataStubs.ThreeDummyRecipes();

            List<Food> recipeFoods = seededRecipes
                .SelectMany(x => x.Ingredients)
                .Select(x => x.Food)
                .Distinct()
                .ToList();
            List<Food> seededFoods = new()
            {
                recipeFoods.Single(x => x.Name == "Test Food 1"),
                recipeFoods.Single(x => x.Name == "Test Food 2"),
                recipeFoods.Single(x => x.Name == "Test Food 3"),
                new Food("Test Food 4")
                {
                    Id = Guid.NewGuid(),
                    Nutrition = new()
                    {
                        Calories = 200
                    },
                    Measurement = new(2, FoodUnit.Liter)
                }
            };

            yield return new object[] 
            {
                seededFoods,
                seededRecipes,
                seededFoods[0].Id.ToString(),
                new List<Recipe>() { seededRecipes[0], seededRecipes[2] }
            };

            yield return new object[]
            {
                seededFoods,
                seededRecipes,
                seededFoods[1].Id.ToString(),
                new List<Recipe>() { seededRecipes[1] }
            };

            yield return new object[]
            {
                seededFoods,
                seededRecipes,
                seededFoods[2].Id.ToString(),
                new List<Recipe>() { seededRecipes[0], seededRecipes[1], seededRecipes[2] }
            };

            yield return new object[]
            {
                seededFoods,
                seededRecipes,
                seededFoods[3].Id.ToString(),
                new List<Recipe>()
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.StaticFiles;
using Pantree.Core.Cooking;
using Pantree.Server.Models.Cooking;
using Pantree.Server.Tests.Utilities;
using Pantree.Server.Tests.Utilities.Database;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Cooking
{
    public class RecipeControllerTests
    {
        private readonly HttpClient _client;
        private readonly PantreeWebApplicationFactory _factory;

        public RecipeControllerTests()
        {
            _factory = new();
            _client = _factory.CreateClient();
        }

        [Theory]
        [MemberData(nameof(GetAllTestData))]
        public async void GetAllTest(List<Recipe> seededRecipes, List<RecipeDto> expectedRecipes)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the basic request; we're expecting an "OK" response at all times
            HttpResponseMessage response = await _client.GetAsync("/api/Recipes/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Check the validity of the response
            List<RecipeDto>? actualRecipes = await response.Content.DeserializeRequestContent<List<RecipeDto>>();
            CustomAssertions.EqualUnordered(expectedRecipes, actualRecipes);
        }

        [Theory]
        [MemberData(nameof(GetSingleTestData))]
        public async void GetSingleTest(List<Recipe> seededRecipes, Guid id, RecipeDto? expectedRecipe)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the request; depending on the test data, we expect either an error response or matching DTO
            HttpResponseMessage response = await _client.GetAsync($"/api/Recipes/{id.ToString()}");
            if (expectedRecipe is null) 
            {
                // Error is expected; validate the API handles that correctly with a NotFound
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            else
            {
                // Matching DTO is expected
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                RecipeDto? actualRecipe = await response.Content.DeserializeRequestContent<RecipeDto>();
                Assert.Equal(expectedRecipe, actualRecipe);
            }
        }

        [Theory]
        [MemberData(nameof(AddOrEditTestData))]
        public async void AddOrEditTest(List<Recipe> seededRecipes, RecipeDto newModel)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the request; the correct response depends on if the entity being "added" already existed or not
            HttpResponseMessage response = await _client.PostAsync("/api/Recipes/", 
                JsonContent.Create<RecipeDto>(newModel));
            RecipeDto? actualRecipe = await response.Content.DeserializeRequestContent<RecipeDto>();

            bool alreadyExists = seededRecipes
                .SingleOrDefault(Recipe => Recipe.Id.ToString() == newModel.Id) is not null;
            if (alreadyExists)
            {
                // The user used the POST endpoint to edit an item
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(newModel, actualRecipe);
            }
            else
            {
                // The user added a new item
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.True(response.Headers.Contains("location"));
                Assert.Equal(response.Headers.GetValues("location").Single(), actualRecipe!.Id);

                newModel.Id = actualRecipe.Id;
                Assert.Equal(newModel, actualRecipe);
            }
        }

        [Theory]
        [MemberData(nameof(AddOrEditTestData))]
        public async void EditTest(List<Recipe> seededRecipes, RecipeDto changedModel)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the request; the correct response depends on if the entity being edited exists or not
            Assert.NotNull(changedModel.Id);
            HttpResponseMessage response = await _client.PutAsync($"/api/Recipes/{changedModel.Id}", 
                JsonContent.Create<RecipeDto>(changedModel));

            // To get a successful response, the ID must already exist. If it doesn't, we expect a NotFound
            bool alreadyExists = seededRecipes
                .SingleOrDefault(Recipe => Recipe.Id.ToString() == changedModel.Id) is not null;
            if (alreadyExists)
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(DeleteTestData))]
        public async void DeleteTest(List<Recipe> seededRecipes, string id)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the request; the correct response depends on if the entity being deleted already existed or not
            HttpResponseMessage response = await _client.DeleteAsync($"/api/Recipes/{id}");

            // If the entity exists, we expect a successful, empty status code; otherwise we expect NotFound
            bool alreadyExists = seededRecipes.SingleOrDefault(Recipe => Recipe.Id.ToString() == id) is not null;
            if (alreadyExists)
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(GetImageTestData))]
        public async void GetImageTest(List<(Recipe, string?)> seededRecipes, string id, int? expectedBytes, string? expectedContentType)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the basic request; we're expecting an "Ok" if the image should exist, and "NoContent" otherwise
            HttpResponseMessage response = await _client.GetAsync($"/api/Recipes/{id}/image");
            if (expectedBytes is not null && expectedContentType is not null)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                byte[] content = await response.Content.ReadAsByteArrayAsync();
                Assert.Equal(expectedBytes, content.Length);
                Assert.Equal(expectedContentType, response.Content.Headers.GetValues("Content-Type").First());
            }
            else
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(SetImageTestData))]
        public async void SetImageTest(List<Recipe> seededRecipes, string id, string imagePath)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Prepare the request content; for this endpoint, that means a form with an "image" field containing the
            // binary data and correctly labeled with the content type
            MultipartFormDataContent formContent = new();
            ByteArrayContent content = new(File.ReadAllBytes(imagePath));
            if (new FileExtensionContentTypeProvider().TryGetContentType(imagePath, out string? contentType))
                content.Headers.ContentType = new(contentType);
            formContent.Add(content, "image", "image");

            // Make the basic request; we're expecting a "NoContent" result
            HttpResponseMessage response = await _client.PostAsync($"/api/Recipes/{id}/image", formContent);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        
        [Theory]
        [MemberData(nameof(DeleteImageTestData))]
        public async void DeleteImageTest(List<(Recipe, string?)> seededRecipes, string id, bool expectError)
        {
            // Seed the database
            SeedData.SeedRecipes(_factory, seededRecipes);

            // Make the basic request; we're expecting a "NoContent" if the image should exist, and "BadRequest" otherwise
            HttpResponseMessage response = await _client.DeleteAsync($"/api/Recipes/{id}/image");
            if (expectError)
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            else
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public static IEnumerable<object?[]> GetAllTestData()
        {
            List<Recipe> recipes = CookingDataStubs.ThreeDummyRecipes();

            yield return new object?[]
            {
                new List<Recipe>(),
                new List<RecipeDto>()
            };
            yield return new object?[]
            {
                new List<Recipe>() { recipes[0] },
                new List<RecipeDto>() { GetExpectedDto(recipes[0]) }
            };
            yield return new object?[]
            {
                new List<Recipe>(recipes),
                recipes.Select(x => GetExpectedDto(x)).ToList()
            };
        }
    
        public static IEnumerable<object?[]> GetSingleTestData()
        {
            List<Recipe> recipes = CookingDataStubs.ThreeDummyRecipes();

            yield return new object?[]
            {
                recipes,
                recipes[0].Id,
                GetExpectedDto(recipes[0])
            };
        }

        public static IEnumerable<object?[]> AddOrEditTestData()
        {
            List<Recipe> seedRecipes = CookingDataStubs.ThreeDummyRecipes();
            // We explicitly reuse the foods that will be seeded in the database here, for multiple reasons:
            //  - It is the most accurate depiction of intended usage; foods _can_ be created newly via this endpoint,
            //    but should generally be created a priori via the dedicated `foods/` endpoint
            //  - There are logistical concerns introduced by doing so; we need to ensure that we can have multiple
            //    recipes using the same foods.
            List<Food> exampleFoods = seedRecipes
                .SelectMany(x => x.Ingredients.Select(x => x.Food))
                .ToList();

            yield return new object?[] // add new recipe
            {
                seedRecipes,
                GetExpectedDto(new Recipe()
                {
                    Id = Guid.NewGuid(),
                    Name = "New recipe.",
                    Description = "A new recipe; definitely not the same as any stub.",
                    Instructions = new()
                    {
                        "One.",
                        "Two."
                    },
                    Ingredients = new()
                    {
                        new Ingredient(exampleFoods[0], new(5, FoodUnit.Unit))
                        {
                            Id = Guid.NewGuid()
                        }
                    },
                    Servings = 3,
                    PreparationTime = TimeSpan.FromMinutes(5),
                    CookingTime = TimeSpan.FromMinutes(30)
                })
            };

            Recipe copyOfExisting = seedRecipes[2] with { };
            copyOfExisting.Name = "A recipe that changed!";
            copyOfExisting.Ingredients = new()
            {
                new Ingredient(exampleFoods[0], new(2, FoodUnit.Pound))
                {
                    Id = Guid.NewGuid()
                }
            };
            yield return new object?[]
            {
                seedRecipes,
                GetExpectedDto(copyOfExisting)
            };
        }

        public static IEnumerable<object?[]> DeleteTestData()
        {
            List<Recipe> recipes = CookingDataStubs.ThreeDummyRecipes();

            yield return new object[] { recipes, recipes[0].Id.ToString() };
            yield return new object[] { recipes, recipes[1].Id.ToString() };
            yield return new object[] { recipes, Guid.NewGuid().ToString() };
        }

        public static IEnumerable<object?[]> GetImageTestData()
        {
            List<(Recipe recipe, string? image)> recipes = CookingDataStubs.ThreeDummyRecipes()
                .Zip(new string?[]
                {
                    null,
                    Path.Join("Test Data", "images", "recipe-sample.jpg"),
                    Path.Join("Test Data", "images", "recipe-sample.webp")
                })
                .ToList();

            yield return new object?[] { recipes, recipes[0].recipe.Id.ToString(), null, null };
            yield return new object?[] { recipes, recipes[1].recipe.Id.ToString(), 8018, "image/jpeg" };
            yield return new object?[] { recipes, recipes[2].recipe.Id.ToString(), 10630, "image/webp" };
        }

        public static IEnumerable<object?[]> SetImageTestData()
        {
            List<Recipe> recipes = CookingDataStubs.ThreeDummyRecipes();
            string testJpeg = Path.Join("Test Data", "images", "recipe-sample.jpg");
            string testWebp = Path.Join("Test Data", "images", "recipe-sample.webp");

            yield return new object[] { recipes, recipes[0].Id.ToString(), testJpeg };
            yield return new object[] { recipes, recipes[0].Id.ToString(), testWebp };
        }

        public static IEnumerable<object?[]> DeleteImageTestData()
        {
            List<(Recipe recipe, string? image)> recipes = CookingDataStubs.ThreeDummyRecipes()
                .Zip(new string?[]
                {
                    null,
                    Path.Join("Test Data", "images", "recipe-sample.jpg"),
                    Path.Join("Test Data", "images", "recipe-sample.webp")
                })
                .ToList();

            yield return new object?[] { recipes, recipes[0].recipe.Id.ToString(), true };
            yield return new object?[] { recipes, recipes[1].recipe.Id.ToString(), false };
            yield return new object?[] { recipes, recipes[2].recipe.Id.ToString(), false };
        }

        private static RecipeDto GetExpectedDto(Recipe recipe)
        {
            return new RecipeDto()
            {
                Id = recipe.Id.ToString(),
                Name = recipe.Name,
                Description = recipe.Description,
                Instructions = new(recipe.Instructions),
                Ingredients = recipe.Ingredients.Select(ingredient => 
                {
                    return new IngredientDto
                    (
                        new FoodDto()
                        {
                            Id = ingredient.Food.Id.ToString(),
                            Name = ingredient.Food.Name,
                            Nutrition = ingredient.Food.Nutrition,
                            Measurement = new FoodMeasurementDto()
                            {
                                Value = ingredient.Food.Measurement?.Value ?? 1,
                                Unit = ingredient.Food.Measurement?.Unit.GetFriendlyName() ?? "unit"
                            }
                        },
                        new FoodMeasurementDto()
                        {
                            Value = ingredient.Quantity.Value,
                            Unit = ingredient.Quantity.Unit.GetFriendlyName()
                        }
                    )
                    {
                        Id = ingredient.Id.ToString(),
                        Nutrition = ingredient.Nutrition
                    };
                }).ToList(),
                Servings = recipe.Servings,
                PreparationTime = Convert.ToUInt32(recipe.PreparationTime?.Minutes),
                CookingTime = Convert.ToUInt32(recipe.CookingTime?.Minutes),
                TotalTime = Convert.ToUInt32(recipe.TotalTime?.Minutes),
                TotalNutrition = recipe.TotalNutrition,
                NutritionPerServing = recipe.NutritionPerServing
            };
        }
    }
}

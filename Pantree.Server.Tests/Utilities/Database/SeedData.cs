using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pantree.Core.Cooking;
using Pantree.Server.Database;
using Pantree.Server.Database.Entities.Cooking;

namespace Pantree.Server.Tests.Utilities.Database
{
    internal static class SeedData
    {
        internal static void SeedFoods(PantreeWebApplicationFactory factory, List<Food> foods)
        {
            using IServiceScope scope = factory.Server.Services.CreateScope();
            PantreeDataContext context = scope.ServiceProvider.GetRequiredService<PantreeDataContext>();
            
            foreach (Food food in foods)
            {
                if (!context.Foods.Select(x => x.Id).Contains(food.Id))
                    context.Foods.Add(new(food.Name)
                    {
                        Id = food.Id,
                        Nutrition = food.Nutrition,
                        Measurement = food.Measurement
                    });
            }
            context.SaveChanges();
        }

        internal static void SeedRecipes(PantreeWebApplicationFactory factory, List<Recipe> recipes) =>
            SeedRecipes(factory, recipes.Select<Recipe, (Recipe, string?)>(x => (x, null)).ToList());

        internal static void SeedRecipes(
            PantreeWebApplicationFactory factory, List<(Recipe recipe, string? imagePath)> recipes)
        {
            Dictionary<Guid, FoodEntity> foodsById = new();
                
            using IServiceScope scope = factory.Server.Services.CreateScope();
            PantreeDataContext context = scope.ServiceProvider.GetRequiredService<PantreeDataContext>();
            
            foreach ((Recipe recipe, string? imagePath) in recipes)
            {
                List<IngredientEntity> ingredients = new();
                foreach (Ingredient ingredient in recipe.Ingredients)
                {
                    if (!foodsById.ContainsKey(ingredient.Food.Id))
                    {
                        if (context.Foods.Select(x => x.Id).Contains(ingredient.Food.Id))
                        {
                            foodsById[ingredient.Food.Id] = context.Foods
                                .Include(x => x.Ingredients)
                                .Single(x => x.Id == ingredient.Food.Id);
                        }
                        else
                        {
                            foodsById[ingredient.Food.Id] = new FoodEntity(
                                ingredient.Food.Name,
                                ingredient.Food.Nutrition ?? new(),
                                ingredient.Food.Measurement ?? new(1, FoodUnit.Unit)
                            ) { Id = ingredient.Food.Id };
                        }
                    }
                        
                    ingredients.Add(new(foodsById[ingredient.Food.Id], ingredient.Quantity)
                    {
                        Id = ingredient.Id
                    });
                }

                byte[]? imageBlob = null;
                string? imageContentType = null;
                if (imagePath is not null)
                {
                    imageBlob = File.ReadAllBytes(imagePath);
                    new FileExtensionContentTypeProvider().TryGetContentType(imagePath, out imageContentType);
                }

                context.Recipes.Add(new RecipeEntity()
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    Description = recipe.Description,
                    Instructions = recipe.Instructions,
                    Ingredients = ingredients,
                    Servings = recipe.Servings,
                    PreparationTime = recipe.PreparationTime,
                    CookingTime = recipe.CookingTime,
                    ImageBlob = imageBlob,
                    ImageContentType = imageContentType
                });    
            }

            context.SaveChanges();
        }
    }
}

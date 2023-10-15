using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pantree.Core.Cooking;
using Pantree.Core.Utilities.Measurement;
using Pantree.Server.Controllers.Search.Providers.FoodDataCentral;

namespace Pantree.Server.Controllers.Search.Providers
{
    /// <summary>
    /// Configuration options for <see cref="FoodDataCentralProvider"/>
    /// </summary>
    public class FoodDataCentralProviderOptions
    {
        /// <summary>
        /// The API key used to make search requests
        /// </summary>
        public string? ApiKey { get; set; } = null;
    }

    /// <summary>
    /// A <see cref="Food"/> search provider that queries the USDA FoodData Central database
    /// </summary>
    public class FoodDataCentralProvider : ISearchProvider<Food>
    {
        /// <summary>
        /// The API key used to interact with FoodData Central
        /// </summary>
        private string _apiKey { get; set; }

        /// <summary>
        /// The service used to issue HTTP requests to FoodData Central
        /// </summary>
        private HttpClient _httpClient { get; set; }

        /// <summary>
        /// A logger dedicated to this class
        /// </summary>
        private ILogger<FoodDataCentralProvider> _logger { get; set; }

        /// <summary>
        /// Construct a new <see cref="FoodDataCentralProvider"/> 
        /// </summary>
        /// <param name="options">Dependency-injected configuration options</param>
        /// <param name="httpClient">Dependency-injected HTTP client used for issuing API requests</param>
        /// <param name="logger">Dependency-injected logger for this class</param>
        /// <exception cref="ArgumentException">
        /// Thrown when no API key is given in <paramref name="options"/>
        /// </exception>
        public FoodDataCentralProvider(
            IOptions<FoodDataCentralProviderOptions> options, 
            HttpClient httpClient,
            ILogger<FoodDataCentralProvider> logger)
        {
            if (options.Value.ApiKey is null)
                throw new ArgumentException("An API key for FoodData Central must be specified.");
            _apiKey = options.Value.ApiKey;

            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Food[]> Search(string query, uint numResults = 25, uint pageNumber = 1)
        {
            string queryUrl = BuildQueryUrl(query, numResults, pageNumber);
            FdcSearchResult result = await MakeRequest(queryUrl);
            return ConvertFoodsToPantree(result.Foods ?? Array.Empty<FdcFood>());
        }

        /// <summary>
        /// Build the URL needed to issue the specified search request
        /// </summary>
        /// <param name="query">The name of the food to search for</param>
        /// <param name="numResults">The maximum number of results to retrieve</param>
        /// <param name="pageNumber">The page number of all results to get</param>
        /// <param name="strict">
        /// Should the search limit results to only items that include every word in <paramref name="query"/>?
        /// </param>
        /// <returns>The URL to send the API request to</returns>
        private string BuildQueryUrl(string query, uint numResults, uint pageNumber, bool strict = true)
        {
            numResults = Math.Clamp(numResults, 1, 200);
            pageNumber = Math.Clamp(pageNumber, 1, uint.MaxValue);

            StringBuilder queryUrlBuilder = new(_httpClient.BaseAddress?.ToString());
            queryUrlBuilder.Append($"?api_key={_apiKey}");
            queryUrlBuilder.Append($"&query={query}");
            queryUrlBuilder.Append($"&pageSize={numResults}");
            queryUrlBuilder.Append($"&pageNumber={pageNumber}");
            if (strict)
                queryUrlBuilder.Append($"&requireAllWords=true");

            return queryUrlBuilder.ToString();
        }

        /// <summary>
        /// Issue the API request to FoodData Central as specified by <paramref name="queryUrl"/>
        /// </summary>
        /// <param name="queryUrl">The full query URL, including parameters</param>
        /// <returns>The <see cref="FdcSearchResult"/></returns>
        /// <exception cref="ArgumentException">Thrown when the query does not return a valid response</exception>
        private async Task<FdcSearchResult> MakeRequest(string queryUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(queryUrl);
            string rawJson = await response.Content.ReadAsStringAsync();

            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };
            FdcSearchResult result = JsonSerializer.Deserialize<FdcSearchResult>(rawJson, jsonOptions)
                ?? throw new ArgumentException($"The query to `{queryUrl}` failed to return valid results.");

            return result;
        }

        /// <summary>
        /// Convert foods returned by FoodData Central into the Pantree <see cref="Food"/> representation
        /// </summary>
        /// <param name="fdcFoods">The foods returned by FoodData Central</param>
        /// <returns>The converted foods</returns>
        private Food[] ConvertFoodsToPantree(FdcFood[] fdcFoods)
        {
            return fdcFoods
                .Where(fdcFood => fdcFood.ServingSize is not null) // ignore foods without valid nutritional info
                .Select(fdcFood =>
            {
                string name = fdcFood.BrandName is not null
                    ? $"{fdcFood.Description} ({fdcFood.BrandName})"
                    : fdcFood.Description ?? "";

                double baseQuantity = fdcFood.ServingSize ?? 1;
                FoodUnit baseUnit = fdcFood.ServingSizeUnit?.ToUpperInvariant() switch
                {
                    "G" or "GRM" => FoodUnit.Gram,
                    // TODO: other units
                    _ => FoodUnit.Unit
                };

                Measurement<FoodUnit> baseMeasurement = new(baseQuantity, baseUnit);

                // Nutrients from FoodData Central are given in terms of 100g servings, so we need to rectify that
                // definition with the actual base serving of the food for our own use
                double fdcNutrientCoefficient = 100 / 
                    new FoodUnitConverter().Convert(baseMeasurement, FoodUnit.Gram).Value;

                Nutrition baseNutrition = new();
                foreach (FdcNutrient nutrient in fdcFood.FoodNutrients ?? Array.Empty<FdcNutrient>())
                {
                    string? nutrientKey = nutrient.NutrientName?.ToUpperInvariant();
                    // It's our general expectation that our base units and FDC's match, so we can just adjust the
                    // values provided without conversions. We'll round to the nearest integer to aid clarity for users
                    double? adjustedValue = nutrient.Value is not null
                        ? Math.Round(nutrient.Value.Value / fdcNutrientCoefficient)
                        : null;
                    switch (nutrientKey)
                    {
                        case "ENERGY":
                            baseNutrition.Calories = adjustedValue;
                            break;
                        case "TOTAL LIPID (FAT)":
                            baseNutrition.TotalFat = adjustedValue;
                            break;
                        case "FATTY ACIDS, TOTAL SATURATED":
                            baseNutrition.SaturatedFat = adjustedValue;
                            break;
                        case "FATTY ACIDS, TOTAL TRANS":
                            baseNutrition.TransFat = adjustedValue;
                            break;
                        case "CHOLESTEROL":
                            baseNutrition.Cholesterol = adjustedValue;
                            break;
                        case "SODIUM, NA":
                            baseNutrition.Sodium = adjustedValue;
                            break;
                        case "CARBOHYDRATE, BY DIFFERENCE":
                            baseNutrition.Carbohydrates = adjustedValue;
                            break;
                        case "FIBER, TOTAL DIETARY":
                            baseNutrition.Fiber = adjustedValue;
                            break;
                        case "SUGARS, TOTAL INCLUDING NLEA":
                            baseNutrition.Sugar = adjustedValue;
                            break;
                        case "PROTEIN":
                            baseNutrition.Protein = adjustedValue;
                            break;
                        default:
                            _logger.LogDebug("Ignored nutrient information: {}", nutrientKey);
                            break;
                    }
                }

                return new Food(name)
                {
                    Nutrition = baseNutrition,
                    Measurement = baseMeasurement
                };
            }).ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Pantree.Server.Models.Cooking;
using Pantree.Server.Tests.Utilities;
using Xunit;

namespace Pantree.Server.Tests.Controllers.Search
{
    public class SearchControllerTests
    {
        private readonly HttpClient _client;
        private readonly PantreeWebApplicationFactory _factory;

        public SearchControllerTests()
        {
            _factory = new();
            _client = _factory.CreateClient();
        }

        /// <summary>
        /// A simple test for the FoodData Central search provider as integrated into the Pantree search endpoint
        /// </summary>
        /// <remarks>
        /// This test is obviously a bit poorly conceived, for multiple reasons. If it becomes problematic, someone
        /// should seriously consider addressing the following:
        /// <list type="bullet">
        ///     <item>
        ///     The test assumes the FoodData Central provider is the currently active search provider, and makes no
        ///     effort to guarantee that assumption is true. The architecture technically allows for this to be flawed,
        ///     though as of this writing no other search providers exist for foods.
        ///     </item>
        ///     <item>
        ///     We're testing an external resource directly -- if FoodData Central's database changes in a way that
        ///     effects our test queries, or if it goes offline, this test will fail without any change on our end.
        ///     This would be best rectified by mocking the external service, or alternatively changing the search
        ///     provider to use a local cache of the FoodData Central database (which is fully downloadable) instead
        ///     of reaching out to its API such that the response is fully within our control.
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="query">The query passed to the Pantree food search endpoint</param>
        /// <param name="expectedFoods">Our expected search result</param>
        [Theory]
        [MemberData(nameof(SearchFoodsFdcTestData))]
        public async void SearchFoodsFdcTest(string query, FoodDto[] expectedFoods)
        {
            // Make the request; we always expect an OK response
            HttpResponseMessage response = await _client.GetAsync($"/api/search/foods/{query}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Evaluate if the request returned what we expected
            //  It's worth noting that we explicitly will not compare the IDs of the returned foods, as they are
            //  randomized by design and that is expected.
            FoodDto[] actualFoods = await response.Content.DeserializeRequestContent<FoodDto[]>() 
                ?? Array.Empty<FoodDto>();
            foreach ((FoodDto expected, FoodDto actual) in expectedFoods.Zip(actualFoods))
                expected.Id = actual.Id;
            Assert.Equal(expectedFoods, actualFoods);
        }

        public static IEnumerable<object?[]> SearchFoodsFdcTestData()
        {
            yield return new object?[]
            {
                "artesano bread",
                new FoodDto[]
                {
                    new()
                    {
                        Name = "ARTESANO THE ORIGINAL BAKERY BREAD (SARA LEE)",
                        Measurement = new()
                        {
                            Unit = "gram",
                            Value = 38
                        },
                        Nutrition = new()
                        {
                            Calories = 110,
                            TotalFat = 2,
                            SaturatedFat = 0,
                            TransFat = 0,
                            Cholesterol = 0,
                            Sodium = 200,
                            Carbohydrates = 20,
                            Fiber = 1,
                            Sugar = 2,
                            Protein = 3
                        }
                    }
                }
            };
        }
    }
}

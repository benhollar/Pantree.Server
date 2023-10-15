using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Pantree.Core.Cooking;
using Pantree.Server.Controllers.Search.Providers;
using Pantree.Server.Database;
using Pantree.Server.Models.Cooking;

namespace Pantree.Server.Controllers.Search
{
    /// <summary>
    /// An API controller used to search for various entities, often using external services
    /// </summary>
    [ApiVersion("1.0")]
    public class SearchController : BaseController
    {
        /// <summary>
        /// The dependency-injected search provider for externally-sourced <see cref="Food"/> items
        /// </summary>
        private ISearchProvider<Food> _foodSearchProvider;

        /// <summary>
        /// The dependency-injected <see cref="IMapper"/> used to translate entities to their respective DTOs
        /// </summary>
        private IMapper _mapper;

        /// <summary>
        /// Construct a new <see cref="SearchController"/> 
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="foodSearchProvider">The food search provider</param>
        /// <param name="mapper">An object mapper</param>
        public SearchController(PantreeDataContext context, ISearchProvider<Food> foodSearchProvider, IMapper mapper)
            : base(context)
        {
            _foodSearchProvider = foodSearchProvider;
            _mapper = mapper;
        }

        /// <summary>
        /// Search for foods of a given name using configured the food search provider
        /// </summary>
        /// <param name="query">The name (or fragment of a name) of the food to search for</param>
        /// <returns>The foods matched by the search query, with a max of 25 results</returns>
        [HttpGet("foods/{query}")]
        public async Task<IActionResult> SearchFoods(string query)
        {
            Food[] foods = await _foodSearchProvider.Search(query, numResults: 25);
            IEnumerable<FoodDto> results = foods.Select(food => _mapper.Map<FoodDto>(food));
            return Ok(results);
        }
    }
}

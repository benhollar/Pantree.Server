using System.Threading.Tasks;

namespace Pantree.Server.Controllers.Search.Providers
{
    /// <summary>
    /// A basic description of any search provider
    /// </summary>
    /// <typeparam name="TSearch">The type of entity to search for</typeparam>
    public interface ISearchProvider<TSearch>
    {
        /// <summary>
        /// Asynchronously search for <typeparamref name="TSearch"/> entities matching a <paramref name="query"/>
        /// </summary>
        /// <param name="query">The search input used to filter entities</param>
        /// <param name="numResults">The maximum number of results to return</param>
        /// <param name="pageNumber">The starting page number of matches to return</param>
        /// <returns>The <typeparamref name="TSearch"/> entities found in the search</returns>
        Task<TSearch[]> Search(string query, uint numResults = 25, uint pageNumber = 0);
    }
}

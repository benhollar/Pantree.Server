using Microsoft.AspNetCore.Mvc;
using Pantree.Server.Database;

namespace Pantree.Server.Controllers
{
    /// <summary>
    /// The fundamental, base definition of an API controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseController : Controller
    {
        /// <summary>
        /// A dependency-injected <see cref="PantreeDataContext"/> to be used for interacting with the backing database
        /// </summary>
        protected readonly PantreeDataContext _context;

        /// <summary>
        /// Construct a new <see cref="BaseController"/>
        /// </summary>
        /// <param name="context">
        /// The configured <see cref="PantreeDataContext"/> provided via dependency injection
        /// </param>
        public BaseController(PantreeDataContext context)
        {
            _context = context;
        }
    }
}

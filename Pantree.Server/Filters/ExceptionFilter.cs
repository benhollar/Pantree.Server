using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Pantree.Server.Filters
{
    /// <summary>
    /// An <see cref="IExceptionFilter"/> meant to catch all uncaught exceptions and produce user-friendly API response
    /// results for user consumption
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
        /// <summary>
        /// The logger dedicated to this exception filter
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Construct a new <see cref="ExceptionFilter"/>
        /// </summary>
        /// <param name="logger">The logger for this instance provided via dependency injection</param>
        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            // If we're in a debugging context, we can just throw the exception instead of trying to handle it; doing
            // so should streamline troubleshooting
            if (Debugger.IsAttached)
                throw context.Exception;
            
            // In all contexts, we can log the exception
            _logger.LogError(context.Exception, "An unexpected exception occurred.");

            context.Result = context.Exception switch
            {
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }
    }
}

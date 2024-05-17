using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Pantree.Server.Filters
{
    /// <summary>
    /// A simple filter for logging the approximate runtime of an API request
    /// </summary>
    public class ResponseTimeLoggingFilter : IActionFilter
    {
        /// <summary>
        /// A key used to embed and later retrieve a stopwatch into the data of a request
        /// </summary>
        private const string StopwatchItemKey = "ResponseTime";  

        /// <summary>
        /// The logger dedicated to this filter
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Construct a new <see cref="ResponseTimeLoggingFilter"/>
        /// </summary>
        /// <param name="logger">The logger for this instance provided via dependency injection</param>
        public ResponseTimeLoggingFilter(ILogger<ResponseTimeLoggingFilter> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context) {  
            // Start the timer   
            context.HttpContext.Items[StopwatchItemKey] = Stopwatch.StartNew();  
        } 
        
        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context) {  
            Stopwatch stopwatch = (Stopwatch)context.HttpContext.Items[StopwatchItemKey]!; 
            stopwatch.Stop(); 
            // Calculate the time elapsed   
            var timeElapsed = stopwatch.Elapsed;
            _logger.LogTrace("{} completed in {} seconds.", context.HttpContext.Request, timeElapsed.TotalSeconds);
        }
    }
}

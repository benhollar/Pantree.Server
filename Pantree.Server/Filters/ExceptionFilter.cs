using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pantree.Server.Filters
{
    /// <summary>
    /// An <see cref="IExceptionFilter"/> meant to catch all uncaught exceptions and produce user-friendly API response
    /// results for user consumption
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
        /// <inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            // If we're in a debugging context, we can just throw the exception instead of trying to handle it; doing
            // so should streamline troubleshooting
            if (Debugger.IsAttached)
                throw context.Exception;

            context.Result = context.Exception switch
            {
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }
    }
}

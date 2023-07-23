using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pantree.Server.Models.Interfaces
{
    /// <summary>
    /// The basic interface any data transfer object (DTO) implements
    /// </summary>
    public interface IDto
    {
        /// <summary>
        /// Validate the contents of the DTO, ensuring that it could be successfully converted into its related model
        /// </summary>
        /// <param name="errorMessages">
        /// A collection of messages describing conditions that caused validation to fail, if any. Provide every known
        /// failure when possible, not just the first detected error.
        /// </param>
        /// <returns>True when validation is successful, false otherwise</returns>
        bool Validate([NotNullWhen(false)] out List<string>? errorMessages);
    }
}

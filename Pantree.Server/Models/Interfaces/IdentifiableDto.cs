namespace Pantree.Server.Models.Interfaces
{
    /// <summary>
    /// An extension of <see cref="IDto"/> describing any DTO with an identifier
    /// </summary>
    public interface IdentifiableDto : IDto
    {
        /// <summary>
        /// The unique identifier for this DTO
        /// </summary>
        string? Id { get; set; }
    }
}

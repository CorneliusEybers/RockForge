using RockForge.Domain.Enums;

namespace RockForge.API.Models.Request
{
    public sealed class CreateRockRequest
    {
        public string Title { get; init; } = string.Empty;

        public RockCategory Category { get; init; }

        public DateOnly DueDate { get; init; }

        public string? Note { get; init; }
    }
}

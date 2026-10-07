using RockForge.Domain.Enums;

namespace RockForge.API.Models.Request
{
    public sealed class UpdateRockStatusRequest
    {
        public RockStatus Status { get; init; }
    }
}

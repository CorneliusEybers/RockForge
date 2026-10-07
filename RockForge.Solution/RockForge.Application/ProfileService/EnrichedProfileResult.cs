using RockForge.Domain;

namespace RockForge.Application.ProfileService
{
    public sealed class EnrichedProfileResult
    {
        public ProfileDto? Profile { get; init; }

        public IEnumerable<Rock> Rocks { get; init; } = Enumerable.Empty<Rock>();

        public bool EnrichmentAvailable { get; init; }
    }
}
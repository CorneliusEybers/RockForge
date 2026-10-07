using RockForge.Application.ProfileService;
using RockForge.Domain;

namespace RockForge.API.Models.Response
{
    public sealed class EnrichedProfileResponse
    {
        public ProfileDto? Profile { get; init; }

        public IEnumerable<Rock> Rocks { get; init; } = Enumerable.Empty<Rock>();

        public bool EnrichmentAvailable { get; init; }
    }
}
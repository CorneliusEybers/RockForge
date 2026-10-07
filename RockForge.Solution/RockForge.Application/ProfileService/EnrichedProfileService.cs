using Microsoft.Extensions.Logging;
using RockForge.Application.RockService;

namespace RockForge.Application.ProfileService
{
    public sealed class EnrichedProfileService : IEnrichedProfileService
    {
        private readonly IRockService _rockService;
        private readonly IProfileClient _profileClient;
        private readonly ILogger<EnrichedProfileService> _logger;

        public EnrichedProfileService(IRockService rockService,
                                      IProfileClient profileClient,
                                      ILogger<EnrichedProfileService> logger)
        {
            _rockService = rockService;
            _profileClient = profileClient;
            _logger = logger;
        }

        public async Task<EnrichedProfileResult> GetEnrichedProfileAsync(string memberId, CancellationToken cancellationToken)
        {
            var rocks = await _rockService.GetByMemberAsync(memberId, null, cancellationToken);

            ProfileDto? profile = null;

            try
            {
                profile = await _profileClient.GetProfileAsync(memberId, cancellationToken);
            }
            catch (Exception exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "Profile enrichment unavailable for member {MemberId}", memberId);
            }

            return new EnrichedProfileResult
            {
                Profile = profile,
                Rocks = rocks.ToList(),
                EnrichmentAvailable = profile != null
            };
        }
    }
}
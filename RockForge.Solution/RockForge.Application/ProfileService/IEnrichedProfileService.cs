namespace RockForge.Application.ProfileService
{
    public interface IEnrichedProfileService
    {
        Task<EnrichedProfileResult> GetEnrichedProfileAsync(string memberId, CancellationToken cancellationToken);
    }
}
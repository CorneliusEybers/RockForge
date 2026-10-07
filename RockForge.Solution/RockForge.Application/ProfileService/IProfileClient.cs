namespace RockForge.Application.ProfileService
{
    public interface IProfileClient
    {
        Task<ProfileDto?> GetProfileAsync(string memberId, CancellationToken cancellationToken);
    }
}
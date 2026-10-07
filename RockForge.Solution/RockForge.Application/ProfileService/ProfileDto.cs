namespace RockForge.Application.ProfileService
{
    public sealed class ProfileDto
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Username { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string Phone { get; init; } = string.Empty;

        public string Website { get; init; } = string.Empty;
    }
}
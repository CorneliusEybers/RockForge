using RockForge.Application.ProfileService;
using System.Net.Http.Json;

namespace RockForge.Infrastructure.ProfileClient
{
    public sealed class JsonPlaceholderProfileClient : IProfileClient
    {
        private readonly HttpClient _httpClient;

        public JsonPlaceholderProfileClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProfileDto?> GetProfileAsync(string memberId,
                                                       CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync($"users/{memberId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProfileDto>(cancellationToken);
        }
    }
}
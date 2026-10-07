using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RockForge.Application.ProfileService;
using RockForge.Domain;
using RockForge.Domain.Enums;
using RockForge.Tests.TestHelpers;

namespace RockForge.Tests
{
    [TestFixture]
    public sealed class EnrichedProfileTests
    {
        [Test]
        public async Task GetEnrichedProfileAsync_ProfileAvailable_ReturnsProfileAndRocks()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = CreateRock();

            await rockService.CreateAsync(rock, CancellationToken.None);

            var profileClient = new SuccessfulProfileClient();

            var service = new EnrichedProfileService(rockService, profileClient, NullLogger<EnrichedProfileService>.Instance);

            // Act
            var result = await service.GetEnrichedProfileAsync("1", CancellationToken.None);

            // Assert
            Assert.That(result.EnrichmentAvailable, Is.True);
            Assert.That(result.Profile, Is.Not.Null);
            Assert.That(result.Profile!.Id, Is.EqualTo(1));
            Assert.That(result.Rocks.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetEnrichedProfileAsync_ProfileUnavailable_ReturnsRocksWithoutProfile()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = CreateRock();

            await rockService.CreateAsync(rock, CancellationToken.None);

            var profileClient = new FailingProfileClient();

            var service = new EnrichedProfileService(rockService, profileClient, NullLogger<EnrichedProfileService>.Instance);

            // Act
            var result = await service.GetEnrichedProfileAsync("1", CancellationToken.None);

            // Assert
            Assert.That(result.EnrichmentAvailable, Is.False);
            Assert.That(result.Profile, Is.Null);
            Assert.That(result.Rocks.Count(), Is.EqualTo(1));
        }

        private static Rock CreateRock()
        {
            return new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = "1",
                Title = "Profile Test Rock",
                Category = RockCategory.Other,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
            };
        }

        private sealed class SuccessfulProfileClient : IProfileClient
        {
            public Task<ProfileDto?> GetProfileAsync(string memberId, CancellationToken cancellationToken)
            {
                ProfileDto profile = new ProfileDto
                {
                    Id = 1,
                    Name = "Test Member",
                    Username = "testmember",
                    Email = "test@example.com",
                    Phone = "0123456789",
                    Website = "example.com"
                };

                return Task.FromResult<ProfileDto?>(profile);
            }
        }

        private sealed class FailingProfileClient : IProfileClient
        {
            public Task<ProfileDto?> GetProfileAsync(string memberId, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("External profile service unavailable.");
            }
        }
    }
}
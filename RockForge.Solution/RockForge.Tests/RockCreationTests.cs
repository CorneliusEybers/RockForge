using NUnit.Framework;
using RockForge.Application.Exceptions;
using RockForge.Domain;
using RockForge.Domain.Enums;
using RockForge.Tests.TestHelpers;

namespace RockForge.Tests
{
    [TestFixture]
    public sealed class RockCreationTests
    {
        [Test]
        public async Task CreateAsync_ValidRock_CreatesPendingRock()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = "1",
                Title = "Complete assessment",
                Category = RockCategory.Other,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
            };

            // Act
            var result = await rockService.CreateAsync(rock, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(rock.Id));
            Assert.That(result.MemberId, Is.EqualTo("1"));
            Assert.That(result.Status, Is.EqualTo(RockStatus.Pending));
        }

        [Test]
        public async Task CreateAsync_HealthRockWithShortTitle_ThrowsValidationException()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = "1",
                Title = "Short",
                Category = RockCategory.Health,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
            };

            // Act / Assert
            var exception = await Assert.ThrowsAsync<RockValidationException>(async () => await rockService.CreateAsync(rock, CancellationToken.None));

            Assert.That(
                exception!.Message,
                Does.Contain("at least 10 characters"));
        }
    }
}
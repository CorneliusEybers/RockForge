using NUnit.Framework;
using RockForge.Domain;
using RockForge.Domain.Enums;
using RockForge.Tests.TestHelpers;

namespace RockForge.Tests
{
    [TestFixture]
    public sealed class RockRetrievalTests
    {
        [Test]
        public async Task GetByMemberAsync_ReturnsOnlyRocksForRequestedMember()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var memberOneRock = CreateRock("1", "Member One Rock");
            var memberTwoRock = CreateRock("2", "Member Two Rock");

            await rockService.CreateAsync(memberOneRock, CancellationToken.None);

            await rockService.CreateAsync(memberTwoRock, CancellationToken.None);

            // Act
            var result = await rockService.GetByMemberAsync("1", null, CancellationToken.None);

            var rocks = result.ToList();

            // Assert
            Assert.That(rocks, Has.Count.EqualTo(1));
            Assert.That(rocks[0].MemberId, Is.EqualTo("1"));
            Assert.That(rocks[0].Id, Is.EqualTo(memberOneRock.Id));
        }

        [Test]
        public async Task GetByMemberAsync_WithStatusFilter_ReturnsMatchingRocksOnly()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var pendingRock = CreateRock("1", "Pending Rock");
            var completedRock = CreateRock("1", "Completed Rock");

            await rockService.CreateAsync(pendingRock, CancellationToken.None);

            await rockService.CreateAsync(completedRock, CancellationToken.None);

            await rockService.UpdateStatusAsync("1", completedRock.Id, RockStatus.Completed, CancellationToken.None);

            // Act
            var result = await rockService.GetByMemberAsync("1", RockStatus.Completed, CancellationToken.None);

            var rocks = result.ToList();

            // Assert
            Assert.That(rocks, Has.Count.EqualTo(1));
            Assert.That(rocks[0].Id, Is.EqualTo(completedRock.Id));
            Assert.That(rocks[0].Status, Is.EqualTo(RockStatus.Completed));
        }

        private static Rock CreateRock(string memberId, string title)
        {
            return new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                Title = title,
                Category = RockCategory.Other,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
            };
        }
    }
}
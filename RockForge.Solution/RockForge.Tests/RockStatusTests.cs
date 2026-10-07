using NUnit.Framework;
using RockForge.Domain;
using RockForge.Domain.Enums;
using RockForge.Domain.Exceptions;
using RockForge.Tests.TestHelpers;

namespace RockForge.Tests
{
    [TestFixture]
    public sealed class RockStatusTests
    {
        [Test]
        public async Task UpdateStatusAsync_PendingToCompleted_UpdatesStatus()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = CreateRock();

            await rockService.CreateAsync(rock, CancellationToken.None);

            // Act
            var result = await rockService.UpdateStatusAsync(rock.MemberId, rock.Id, RockStatus.Completed, CancellationToken.None);

            // Assert
            Assert.That(result.Status, Is.EqualTo(RockStatus.Completed));
        }

        [Test]
        public async Task UpdateStatusAsync_CompletedToMissed_ThrowsInvalidStateTransition()
        {
            // Arrange
            var rockService = RockServiceFactory.Create();

            var rock = CreateRock();

            rockService.CreateAsync(rock, CancellationToken.None)
                       .GetAwaiter()
                       .GetResult();

            rockService.UpdateStatusAsync(rock.MemberId, rock.Id, RockStatus.Completed, CancellationToken.None)
                       .GetAwaiter()
                       .GetResult();

            // Act / Assert
            var exception = await Assert.ThrowsAsync<InvalidRockStateTransitionException>(async () => await rockService.UpdateStatusAsync(rock.MemberId,
                                                                                                                                          rock.Id,
                                                                                                                                          RockStatus.Missed,
                                                                                                                                          CancellationToken.None));

            Assert.That(exception!.Message, Does.Contain("Completed"));
        }

        private static Rock CreateRock()
        {
            return new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = "1",
                Title = "Status Test Rock",
                Category = RockCategory.Other,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
            };
        }
    }
}
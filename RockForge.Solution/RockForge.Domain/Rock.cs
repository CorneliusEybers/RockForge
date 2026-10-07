using RockForge.Domain.Enums;
using RockForge.Domain.Exceptions;

namespace RockForge.Domain
{
    public sealed class Rock
    {
        public Guid Id { get; init; }

        public string MemberId { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public RockCategory Category { get; init; }

        public DateOnly DueDate { get; init; }

        public string? Note { get; init; }

        public RockStatus Status { get; private set; } = RockStatus.Pending;

        public void UpdateStatus(RockStatus status)
        {
            if (Status != RockStatus.Pending)
            {
                throw new InvalidRockStateTransitionException($"Rock status cannot transition from '{Status}' to '{status}'.");
            }

            if (status != RockStatus.Completed 
                &&
                status != RockStatus.Missed)
            {
                throw new InvalidRockStateTransitionException("A pending Rock may only transition to Completed or Missed.");
            }

            Status = status;
        }
    }
}

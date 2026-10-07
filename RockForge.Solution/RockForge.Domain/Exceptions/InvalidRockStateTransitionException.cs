namespace RockForge.Domain.Exceptions
{
    public sealed class InvalidRockStateTransitionException : Exception
    {
        public InvalidRockStateTransitionException(string message) : base(message)
        {
        }
    }
}
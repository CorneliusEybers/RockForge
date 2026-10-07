namespace RockForge.Application.Exceptions
{
    public sealed class RockValidationException : Exception
    {
        public RockValidationException(string message) : base(message)
        {
        }
    }
}
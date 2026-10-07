namespace RockForge.Application.Exceptions
{
    public sealed class RockNotFoundException : Exception
    {
        public RockNotFoundException(string message) : base(message)
        {
        }
    }
}
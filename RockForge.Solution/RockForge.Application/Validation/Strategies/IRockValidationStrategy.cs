using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation.Strategies
{
    public interface IRockValidationStrategy
    {
        RockCategory Category { get; }

        void Validate(Rock rock);
    }
}
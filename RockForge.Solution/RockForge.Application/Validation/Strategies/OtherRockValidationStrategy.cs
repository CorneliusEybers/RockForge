using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation.Strategies
{
    public sealed class OtherRockValidationStrategy : IRockValidationStrategy
    {
        public RockCategory Category => RockCategory.Other;

        public void Validate(Rock rock)
        {
            // No additional category validation required.
        }
    }
}
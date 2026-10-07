using RockForge.Application.Exceptions;
using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation.Strategies
{
    public sealed class HealthRockValidationStrategy : IRockValidationStrategy
    {
        public RockCategory Category => RockCategory.Health;

        public void Validate(Rock rock)
        {
            if (rock.Title.Length < 10)
            {
                throw new RockValidationException(
                    "Health Rock title must be at least 10 characters.");
            }
        }
    }
}
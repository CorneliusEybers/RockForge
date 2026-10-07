using RockForge.Application.Exceptions;
using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation.Strategies
{
    public sealed class CareerRockValidationStrategy : IRockValidationStrategy
    {
        public RockCategory Category => RockCategory.Career;

        public void Validate(Rock rock)
        {
            if (string.IsNullOrWhiteSpace(rock.Note))
            {
                throw new RockValidationException(
                    "Career Rock requires a note explaining why it matters.");
            }
        }
    }
}
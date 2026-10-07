using RockForge.Application.Exceptions;
using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation.Strategies
{
    public sealed class RevenueRockValidationStrategy : IRockValidationStrategy
    {
        public RockCategory Category => RockCategory.Revenue;

        public void Validate(Rock rock)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var currentQuarter = ((today.Month - 1) / 3) + 1;

            var quarterStartMonth = ((currentQuarter - 1) * 3) + 1;

            var quarterStart = new DateOnly(
                today.Year,
                quarterStartMonth,
                1);

            var quarterEnd = quarterStart
                .AddMonths(3)
                .AddDays(-1);

            if (rock.DueDate < quarterStart ||
                rock.DueDate > quarterEnd)
            {
                throw new RockValidationException(
                    "Revenue Rock due date must fall within the current quarter.");
            }
        }
    }
}
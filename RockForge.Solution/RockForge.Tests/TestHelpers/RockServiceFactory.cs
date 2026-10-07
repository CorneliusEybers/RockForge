using RockForge.Application.RockService;
using RockForge.Application.Validation.Strategies;

namespace RockForge.Tests.TestHelpers
{
    internal static class RockServiceFactory
    {
        public static IRockService Create()
        {
            IRockValidationStrategy[] strategies =
            [
                new RevenueRockValidationStrategy(),
                new HealthRockValidationStrategy(),
                new CareerRockValidationStrategy(),
                new OtherRockValidationStrategy()
            ];

            return new RockService(strategies);
        }
    }
}
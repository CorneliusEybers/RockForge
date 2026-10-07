using RockForge.Application.Exceptions;
using RockForge.Application.Validation;
using RockForge.Application.Validation.Strategies;
using RockForge.Domain;
using RockForge.Domain.Enums;
using System.Collections.Concurrent;


namespace RockForge.Application.RockService
{
    public class RockService : IRockService
    {
        private readonly ConcurrentDictionary<Guid, Rock> _rocks = new();

        private readonly IEnumerable<IRockValidationStrategy> _validationStrategies;

        public RockService(IEnumerable<IRockValidationStrategy> validationStrategies)
        {
            _validationStrategies = validationStrategies;
        }

        public Task<Rock> CreateAsync(Rock rock, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Base validation
            RockValidator.ValidateForCreate(rock);

            // Category-specific validation
            var validationStrategy = _validationStrategies.Single(strategy => strategy.Category == rock.Category);
            validationStrategy.Validate(rock);

            // - Add the rock to the in-memory store
            _rocks.TryAdd(rock.Id, rock);

            return Task.FromResult(rock);
        }

        public Task<IEnumerable<Rock>> GetByMemberAsync(string memberId, RockStatus? rockStatus, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Rock> rocks = _rocks.Values.Where(r => r.MemberId == memberId);

            if (rockStatus.HasValue)
            {
                rocks = rocks.Where(r => r.Status == rockStatus.Value);
            }

            return Task.FromResult(rocks);
        }

        public Task<Rock> UpdateStatusAsync(string memberId, Guid rockId, RockStatus status, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rock = _rocks.Values.FirstOrDefault(r => r.Id == rockId 
                                                         &&
                                                         r.MemberId == memberId);

            if (rock == null)
            {
                throw new RockNotFoundException($"Rock '{rockId}' was not found for member '{memberId}'.");
            }

            rock.UpdateStatus(status);

            return Task.FromResult(rock);
        }
    }
}

using RockForge.Application.Validation;
using RockForge.Domain;
using RockForge.Domain.Enums;
using System.Collections.Concurrent;

namespace RockForge.Application.RockService
{
    public class RockService : IRockService
    {
        private readonly ConcurrentDictionary<Guid, Rock> _rocks = new();

        public Task<Rock> CreateAsync(Rock rock, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RockValidator.ValidateForCreate(rock);

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
                throw new KeyNotFoundException($"Rock '{rockId}' was not found for member '{memberId}'.");
            }

            rock.UpdateStatus(status);

            return Task.FromResult(rock);
        }
    }
}

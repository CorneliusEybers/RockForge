using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.RockService
{
    public interface IRockService
    {
        Task<Rock> CreateAsync(Rock rock, CancellationToken cancellationToken);
        Task<IEnumerable<Rock>> GetByMemberAsync(string memberId, RockStatus? rockStatus, CancellationToken cancellationToken);
        Task<Rock> UpdateStatusAsync(string memberId, Guid rockId, RockStatus status, CancellationToken cancellationToken);
    }
}

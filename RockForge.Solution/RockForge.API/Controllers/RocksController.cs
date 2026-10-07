using Microsoft.AspNetCore.Mvc;
using RockForge.API.Models.Request;
using RockForge.Application.RockService;
using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.API.Controllers
{
    [ApiController]
    [Route("members/{memberId}/rocks")]
    public sealed class RocksController : ControllerBase
    {
        private readonly IRockService _rockService;

        public RocksController(IRockService rockService)
        {
            _rockService = rockService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Rock), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Rock>> CreateRock(string memberId,
                                                         [FromBody] CreateRockRequest request,
                                                         CancellationToken cancellationToken)
        {
            Rock newRock = new Rock
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                Title = request.Title,
                Category = request.Category,
                DueDate = request.DueDate,
                Note = request.Note
            };

            var rock = await _rockService.CreateAsync(newRock,
                                                      cancellationToken);

            return CreatedAtAction(nameof(GetRocks),
                                   new { memberId },
                                   rock);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Rock>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Rock>>> GetRocks(string memberId,
                                                                    [FromQuery] RockStatus? status,
                                                                    CancellationToken cancellationToken)
        {
            var rocks = await _rockService.GetByMemberAsync(memberId,
                                                            status,
                                                            cancellationToken);

            return Ok(rocks);
        }

        [HttpPatch("{rockId:guid}")]
        [ProducesResponseType(typeof(Rock), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Rock>> UpdateRockStatus(string memberId,
                                                               Guid rockId,
                                                               [FromBody] UpdateRockStatusRequest request,
                                                               CancellationToken cancellationToken)
        {
            var rock = await _rockService.UpdateStatusAsync(memberId,
                                                            rockId,
                                                            request.Status,
                                                            cancellationToken);

            return Ok(rock);
        }
    }
}

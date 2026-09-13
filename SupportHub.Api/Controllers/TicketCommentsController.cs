 using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;

namespace SupportHub.Api.Controllers
{
    // Comments live under the ticket because a comment cannot exist without one.
    // That is containment, and containment goes in the path.
    [ApiController]
    [Route("api/v1/tickets/{ticketId:int}/comments")]
    [Produces("application/json")]
    public class TicketCommentsController : ControllerBase
    {
        private readonly ILogger<TicketCommentsController> _logger;
        private readonly ITicketCommentService _ticketCommentService;
        private readonly ITicketService _ticketService;

        public TicketCommentsController(
            ILogger<TicketCommentsController> logger,
            ITicketCommentService ticketCommentService,
            ITicketService ticketService)
        {
            _logger = logger;
            _ticketCommentService = ticketCommentService;
            _ticketService = ticketService;
        }

        [HttpGet]
        [ProducesResponseType<TicketCommentDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TicketCommentDto>>> GetComments(int ticketId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving comments for ticket {TicketId}", ticketId);
            if (!await _ticketService.TicketExistsAsync(ticketId, cancellationToken))
            {
                return NotFound();
            }
            var comments=await _ticketCommentService.GetCommentsAsync(ticketId,cancellationToken);
            return Ok(comments);

        }
        [HttpGet("{id:int}", Name = nameof(GetCommentById))]
        [ProducesResponseType<TicketCommentDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketCommentDto>> GetCommentById(int ticketId, int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving comment {CommentId} on ticket {TicketId}", id, ticketId);
             var comment=await _ticketCommentService.GetCommentAsync(ticketId, id, cancellationToken);
            if (comment is null)
            {
                return NotFound();
            }
         return Ok(comment);
        }

        [HttpPost]
        [ProducesResponseType<TicketCommentDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketCommentDto>> CreatComment(int  ticketId,[FromBody] TicketCommentCreateDto ticketCommentCreateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding a comment to ticket {TicketId}", ticketId);

            // A missing ticket is a 404. An empty body is a 400: the ticket is fine,
            // the request isn't. [ApiController] has already handled that one.
            if (!await _ticketService.TicketExistsAsync(ticketId, cancellationToken))
            {
                return NotFound();
            }

            if (ticketCommentCreateDto.AuthorAgentId is not null
                && !await _ticketService.AgentExistsAsync(ticketCommentCreateDto.AuthorAgentId.Value, cancellationToken))
            {
                return Problem(
                    detail: $"Agent {ticketCommentCreateDto.AuthorAgentId} does not exist.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unknown agent");
            }

            var created = await _ticketCommentService.CreateCommentAsync(ticketId, ticketCommentCreateDto, cancellationToken);

            return CreatedAtRoute(nameof(GetCommentById), new { ticketId, id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(int ticketId, int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting comment {CommentId} on ticket {TicketId}", id, ticketId);

            if (!await _ticketCommentService.CommentExistsAsync(ticketId, id, cancellationToken))
            {
                return NotFound();
            }

            await _ticketCommentService.DeleteCommentAsync(ticketId, id, cancellationToken);

            return NoContent();
        }
    }
}

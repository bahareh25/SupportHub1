using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;

namespace SupportHub.Api.Controllers
{

    [ApiController]
    [Route("api/v1/tags")]
    [Produces("application/json")]
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly ITagService _tagService;
        private readonly ITicketService _ticketService;

        public TagsController(
            ILogger<TagsController> logger,
            ITagService tagService,
            ITicketService ticketService)
        {
            _logger = logger;
            _tagService = tagService;
            _ticketService = ticketService;
        }
        [HttpGet]
        [ProducesResponseType<IEnumerable<TagDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTags(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tags");

            var tags = await _tagService.GetAllTagsAsync(cancellationToken);

            return Ok(tags);
        }

        [HttpGet("{id:int}", Name = nameof(GetTagById))]
        [ProducesResponseType<TagDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagDto>> GetTagById(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tag {TagId}", id);

            var tag = await _tagService.GetTagByIdAsync(id, cancellationToken);

            if (tag is null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [HttpPost]
        [ProducesResponseType<TagDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] TagCreateDto tagCreateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating a tag");

            // "billing" and "BILLING" are the same tag, so the second one is a conflict.
            if (await _tagService.TagNameExistsAsync(tagCreateDto.Name, cancellationToken))
            {
                return Conflict();
            }

            var created = await _tagService.CreateTagAsync(tagCreateDto, cancellationToken);

            return CreatedAtRoute(nameof(GetTagById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting tag {TagId}", id);

            if (!await _tagService.TagExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }

            await _tagService.DeleteTagAsync(id, cancellationToken);

            return NoContent();
        }

        // The leading "/" escapes this controller's api/v1/tags route.
        // PUT, not POST, because "make sure this tag is on this ticket" is idempotent:
        // run it five times and the answer is the same.
        [HttpPut("/api/v1/tickets/{ticketId:int}/tags/{tagId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AttachTagToTicket(int ticketId, int tagId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attaching tag {TagId} to ticket {TicketId}", tagId, ticketId);
            if (!await _ticketService.TicketExistsAsync(ticketId, cancellationToken))
            { return NotFound(); }
            if (!await _tagService.TagExistsAsync(tagId, cancellationToken))
            {
                return NotFound();
            }
            await _tagService.AttachTagToTicketAsync(ticketId, tagId, cancellationToken);
            return NoContent();

        }
        // No 404 here. The client asked for the tag to be gone; if it was never there,
        // it is still gone, so the answer is the same 204.
        [HttpDelete("/api/v1/tickets/{ticketId:int}/tags/{tagId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DetachTagFromTicket(int ticketId, int tagId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Detaching tag {TagId} from ticket {TicketId}", tagId, ticketId);

            await _tagService.DetachTagFromTicketAsync(ticketId, tagId, cancellationToken);

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;
using System.Reflection.Metadata.Ecma335;

namespace SupportHub.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tickets")]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private readonly ILogger<TicketsController> _logger;
        private readonly ITicketService _ticketService;
        private readonly ICustomerService _customerService;

        public TicketsController(ILogger<TicketsController> logger, ITicketService ticketService, ICustomerService customerService)
        {
            _logger = logger;
            _ticketService = ticketService;
            _customerService = customerService;
        }
        [HttpGet]
        [ProducesResponseType<IEnumerable<TicketDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTickets(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tickets");

            var tickets = await _ticketService.GetAllTicketsAsync(cancellationToken);

            return Ok(tickets);
        }
        [HttpGet("{id:int}", Name = nameof(GetTicketById))]
        [ProducesResponseType<TicketDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketDto>> GetTicketById(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving Ticket by {TicketID}", id);
            var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);
            if (ticket is null)
            { return NotFound(); }
            return Ok(ticket);
        }
        [HttpPost]
        [ProducesResponseType<TicketDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] TicketCreateDto ticketCreateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating a ticket for customer {CustomerId}", ticketCreateDto.CustomerId);
            // A ticket must belong to a customer, so an unknown customer is a bad request.
            if (!await _customerService.CustomerExistsAsync(ticketCreateDto.CustomerId, cancellationToken))
            {
                return Problem(detail: $"Customer {ticketCreateDto.CustomerId} does not exist.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unknown customer"); }

            if (!await _ticketService.AgentExistsAsync(ticketCreateDto.AssignedAgentId.Value, cancellationToken))
            {
                return Problem(
                    detail: $"Agent {ticketCreateDto.AssignedAgentId} does not Exit.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unkown agent");
            }
            var created = await _ticketService.CreateTicketAsync(ticketCreateDto, cancellationToken);

            return CreatedAtRoute(nameof(GetTicketById), new { id = created.Id }, created);
        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] TicketUpdateDto ticketUpdateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("update ticket by {ticketid}", id);
            if (!await _ticketService.TicketExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }
            if (ticketUpdateDto.AssignedAgentId is not null
             && !await _ticketService.AgentExistsAsync(ticketUpdateDto.AssignedAgentId.Value, cancellationToken))
            {
                return Problem(
                    detail: $"Agent {ticketUpdateDto.AssignedAgentId} does not exist.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unknown agent");
            }

            await _ticketService.UpdateTicketAsync(id, ticketUpdateDto, cancellationToken);
            return NoContent();
        }
        [HttpPost("{id:int}/status")]
        [ProducesResponseType<TicketDto>(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketDto>> ChangeTicketStatus(int id, [FromBody] TicketStatusUpdateDto ticketStatusUpdateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Changing status of ticket {TicketId} to {NextStatus}", id, ticketStatusUpdateDto.Status);
            var currentStatus=await _ticketService.GetTicketStatusAsync(id, cancellationToken);
            if (currentStatus is null)
            {
                return NotFound();
            }

            var updated=await _ticketService.TryChangeTicketStatusAsync(id,ticketStatusUpdateDto.Status, cancellationToken);
            if (updated is null)
            {
                return Conflict(new ProblemDetails
                {
                    Type = "https://supporthub.example/problems/invalid-status-transition",
                    Title = "Invalid status transition",
                    Detail = $"A ticket in {currentStatus} cannot move to {ticketStatusUpdateDto.Status}. " +
                          "Allowed: Open -> InProgress -> Resolved -> Closed.",
                    Status = StatusCodes.Status409Conflict,
                    Instance = HttpContext.Request.Path
                });
            }
            // The status change has something worth returning: the new state.
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTicket(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting ticket {TicketId}", id);

            if (!await _ticketService.TicketExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }

            await _ticketService.DeleteTicketAsync(id, cancellationToken);

            return NoContent();
        }
    }
}

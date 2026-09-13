using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Application.Services;

namespace SupportHub.Api.Controllers
{

    [ApiController]
    [Route("api/v1/customers")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private ILogger<CustomersController> _logger;
        private ICustomerService _customerService;
        private ITicketService _ticketService;

        public CustomersController(
        ILogger<CustomersController> logger,
        ICustomerService customerService,
        ITicketService ticketService)
        {
            _logger = logger;
            _customerService = customerService;
            _ticketService = ticketService;
        }

        [HttpGet]
        [ProducesResponseType<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving customers");
            var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
            // An empty collection is still a collection: 200 with [], never 404.
            return Ok(customers);
        }
        [HttpGet("{id:int}", Name = nameof(GetCustomerById))]
        [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retriving Customer By {CustomerId}", id);
            var customer = await _customerService.GetCustomerByIdAsync(id, cancellationToken);
            if (customer is null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpGet("{id:int}/tickets")]
        [ProducesResponseType<IEnumerable<TicketDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetCustomerTickets(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tickets for customer {CustomerId}", id);

            if (!await _customerService.CustomerExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }

            // Tickets are queried by the ticket service, so only one place knows how to load them.
            var tickets = await _ticketService.GetTicketsByCustomerAsync(id, cancellationToken);

            return Ok(tickets);
        }
        [HttpPost]
        [ProducesResponseType<CustomerDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateDto customerCreateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating a new customer");
            //a duplicate email is well-formed, so it is a confilict,not a bad request.
            if (await _customerService.EmailExistsAsync(customerCreateDto.Email, cancellationToken: cancellationToken))
            {
                return Conflict();
            }
            var created = await _customerService.CreateCustomerAsync(customerCreateDto, cancellationToken);

            //201 carries a  location header  pointing at the new customer.
            return CreatedAtRoute(nameof(GetCustomerById), new { id = created.Id }, created);
        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto customerUpdateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating customer {CustomerId}", id);

            if (!await _customerService.CustomerExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }

            // The customer keeping its own email is not a duplicate.
            if (await _customerService.EmailExistsAsync(customerUpdateDto.Email, id, cancellationToken))
            {
                return Conflict();
            }

            await _customerService.UpdateCustomerAsync(id, customerUpdateDto, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCustomer(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting customer {CustomerId}", id);

            if (!await _customerService.CustomerExistsAsync(id, cancellationToken))
            {
                return NotFound();
            }

            // Restrict on the FK means the database would refuse this anyway.
            if (await _customerService.CustomerHasTicketsAsync(id, cancellationToken))
            {
                return Conflict();
            }

            await _customerService.DeleteCustomerAsync(id, cancellationToken);

            return NoContent();
        } 
    }
}

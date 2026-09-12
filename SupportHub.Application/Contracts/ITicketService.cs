using SupportHub.Application.Dtos;
using SupportHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Application.Contracts;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync(CancellationToken cancellationToken = default);

    Task<TicketDto?> GetTicketByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TicketDto>> GetTicketsByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<bool> TicketExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> AgentExistsAsync(int agentId, CancellationToken cancellationToken = default);

    // Returns null when the ticket does not exist.
    Task<TicketStatus?> GetTicketStatusAsync(int id, CancellationToken cancellationToken = default);

    Task<TicketDto> CreateTicketAsync(TicketCreateDto ticketCreateDto, CancellationToken cancellationToken = default);

    Task UpdateTicketAsync(int id, TicketUpdateDto ticketUpdateDto, CancellationToken cancellationToken = default);

    // Returns null when the status machine does not allow the move.
    Task<TicketDto?> TryChangeTicketStatusAsync(int id, TicketStatus nextStatus, CancellationToken cancellationToken = default);

    Task DeleteTicketAsync(int id, CancellationToken cancellationToken = default);
}

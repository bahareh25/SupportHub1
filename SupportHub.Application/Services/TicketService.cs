using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Enums;
using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Application.Services;

public class TicketService : ITicketService
{
    private IApplicationDbContext _db;
    private ILogger<TicketService> _logger;

    public TicketService(IApplicationDbContext db,ILogger<TicketService> logger)
    {
        _db = db;
        _logger = logger;
    }
    // Every query that ends up as a TicketDto starts here, so the tag include lives
    // in one place instead of being repeated (and eventually forgotten) at each call site.
    // Tracking is left to the caller: reads add AsNoTracking, writes need tracking.
    private IQueryable<Ticket> TicketsWithTags() =>
        _db.Tickets
            .Include(t => t.TicketTags)
            .ThenInclude(tt => tt.Tag);
    public async Task<bool> AgentExistsAsync(int agentId, CancellationToken cancellationToken = default)
    {
        return await _db.Agents.AnyAsync(c=>c.Id == agentId);
    }

    public async Task<TicketDto> CreateTicketAsync(TicketCreateDto ticketCreateDto, CancellationToken cancellationToken = default)
    {
        // Both stamps share one timestamp so a brand-new ticket reads consistently.
        var now = DateTime.UtcNow;
        var ticket = new Ticket 
        { 
            Title= ticketCreateDto.Title.Trim(),
            Description= ticketCreateDto.Description.Trim(),
            CustomerId= ticketCreateDto.CustomerId,
            AssignedAgentId= ticketCreateDto.AssignedAgentId,
            Priority= ticketCreateDto.Priority,
            CreatedAtUtc=now,
            UpdatedAtUtc=now
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ticket {TicketId} for customer {CustomerId}", ticket.Id, ticket.CustomerId);
         return TicketDto.From(ticket);

    }

    public async Task DeleteTicketAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket=await _db.Tickets.FirstOrDefaultAsync(c=>c.Id== id,cancellationToken);
        if (ticket is null)
            return;
        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted ticket {TicketId}", id);
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync(CancellationToken cancellationToken = default)
    {
        var ticket = await TicketsWithTags()
                  .AsNoTracking()
                  .OrderBy(c => c.Id)
                  .ToListAsync(cancellationToken);
        // Mapped after ToListAsync so TicketDto.From runs in memory. Calling it inside
        // a Select would make EF try to translate it into SQL, and fail.
        return ticket.Select(TicketDto.From).ToList();
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketsWithTags()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        return ticket is null? null:TicketDto.From(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var  ticket = await TicketsWithTags()
                        .AsNoTracking()
                        .Where(c=>c.CustomerId == customerId)
                        .OrderBy(t=>t.Id)
                        .ToListAsync(cancellationToken);
        return ticket.Select(TicketDto.From).ToList();

    }

    public async Task<TicketStatus?> GetTicketStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket= await _db.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(c=>c.Id == id,cancellationToken);
        return ticket?.Status;
    }

    public async Task<bool> TicketExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tickets.AnyAsync(c=>c.Id == id,cancellationToken);
    }

    public async Task<TicketDto?> TryChangeTicketStatusAsync(int id, TicketStatus nextStatus, CancellationToken cancellationToken = default)
    {
        // Tracked, so no AsNoTracking here: this method writes.
        var ticket = await TicketsWithTags()
             .FirstOrDefaultAsync(c=> c.Id==id,cancellationToken);
        if (ticket is null)
            return null;
        // The rule lives on the entity, so it is the same rule everywhere.
        if (!ticket.CanTransitionTo(nextStatus))
        {
            _logger.LogInformation(
                "Refused status change on ticket {TicketId}: {CurrentStatus} -> {NextStatus}",
                id, ticket.Status, nextStatus);

            return null;
        }
        ticket.Status= nextStatus;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ticket {TicketId} moved to {NextStatus}", id, nextStatus);
        return TicketDto.From(ticket);
    }

    public async Task UpdateTicketAsync(int id, TicketUpdateDto ticketUpdateDto, CancellationToken cancellationToken = default)
    {
       var ticket= await _db.Tickets.FirstOrDefaultAsync(c=>c.Id == id, cancellationToken);
        if (ticket is null)
            return;
        ticket.Title= ticketUpdateDto.Title.Trim();
        ticket.Description= ticketUpdateDto.Description.Trim();
        ticket.AssignedAgentId= ticketUpdateDto.AssignedAgentId;
        ticket.Priority= ticketUpdateDto.Priority;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ticket {TicketId}", id);

    }
}

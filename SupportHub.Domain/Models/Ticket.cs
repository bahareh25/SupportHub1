using SupportHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Domain.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // A ticket must belong to a customer
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null;

    // A ticket may have an assigned agent.
    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketTag> TicketTags { get; set; } = new List<TicketTag>();

    /// <summary>
    /// The status machine: Open -> InProgress -> Resolved -> Closed.
    /// You may only move forward, one step at a time.
    /// </summary
    public bool CanTransitionTo(TicketStatus next) => (Status, next) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.Resolved, TicketStatus.Closed) => true,
        _ => false
    };
}

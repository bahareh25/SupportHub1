using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Domain.Models;

public class TicketComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int? AuthorAgentId { get; set; }
    public Agent? AuthorAgent { get; set; }
}

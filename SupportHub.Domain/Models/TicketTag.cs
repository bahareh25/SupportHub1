using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Domain.Models;

public class TicketTag
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

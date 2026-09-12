using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Domain.Models;

public class Agent
{
    public int Id { get; set; }
    public string DisplayName { get; set; }
    public  string Email  { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
}

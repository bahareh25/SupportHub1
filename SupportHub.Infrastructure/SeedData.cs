using Microsoft.EntityFrameworkCore;
using SupportHub.Domain.Enums;
using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Infrastructure;

public static class SeedData
{
    public static async Task EnsureSeadedAsync(SupportHubDbContext db)
    {
        // Implementation for seeding data
        await db.Database.MigrateAsync();

        if (await db.Customers.AnyAsync()) return;

        var acme = new Customer { Name = "Acme Logistics", Email = "ops@acme.example" };
        var globex = new Customer { Name = "Globex Retail", Email = "it@globex.example" };

        var priya = new Agent { DisplayName = "Priya N.", Email = "priya@supporthub.example" };
        var tom = new Agent { DisplayName = "Tom R.", Email = "tom@supporthub.example" };

        var billing = new Tag { Name = "billing" };
        var bug = new Tag { Name = "bug" };
        var urgent = new Tag { Name = "urgent" };

        db.AddRange(acme, globex, priya, tom, billing, bug, urgent);
        await db.SaveChangesAsync();

        var t1 = new Ticket
        {
            Title = "Invoice 4471 shows the wrong tax rate",
            Description = "GST is being applied at 13% instead of 5% on Alberta shipments.",
            CustomerId = acme.Id,
            AssignedAgentId = priya.Id,
            Priority = TicketPriority.High
        };

        var t2 = new Ticket
        {
            Title = "Cannot log in after password reset",
            Description = "Reset email arrives, new password is rejected as invalid.",
            CustomerId = globex.Id,
            Priority = TicketPriority.Critical
        };

        db.AddRange(t1, t2);
        await db.SaveChangesAsync();

        db.TicketTags.AddRange(
            new TicketTag { TicketId = t1.Id, TagId = billing.Id },
            new TicketTag { TicketId = t2.Id, TagId = bug.Id },
            new TicketTag { TicketId = t2.Id, TagId = urgent.Id });

        db.TicketComments.Add(new TicketComment
        {
            TicketId = t1.Id,
            AuthorAgentId = priya.Id,
            Body = "Reproduced on staging. Looks like the province lookup is defaulting to ON."
        });

        await db.SaveChangesAsync();
    }
}

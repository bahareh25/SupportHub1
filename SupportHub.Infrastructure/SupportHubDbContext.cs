using Microsoft.EntityFrameworkCore;
using SupportHub.Application.Contracts;
using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Infrastructure;

public class SupportHubDbContext: DbContext,IApplicationDbContext
{
    public SupportHubDbContext(DbContextOptions<SupportHubDbContext> options):base(options)
    {
        
    }
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<TicketTag> TicketTags => Set<TicketTag>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
            {
                e.Property(c => c.Name).IsRequired().HasMaxLength(200);
                e.Property(c => c.Email).IsRequired().HasMaxLength(320);
                e.HasIndex(c => c.Email).IsUnique();
            });
        modelBuilder.Entity<Agent>(e =>
        { 
            e.Property(a=>a.DisplayName).IsRequired().HasMaxLength(200);
            e.Property(a => a.Email).IsRequired().HasMaxLength(320);
        });
        modelBuilder.Entity<Ticket>(e =>
        {
            e.Property(t=>t.Title).IsRequired().HasMaxLength(200);
            e.Property(t => t.Description).HasMaxLength(4000);
            //Required: a ticket must belong to a customer.
            //Restrict, so you cannot silently delete a customer wgo still has tickets.
            e.HasOne(t => t.Customer)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            //optional: an unassigned ticket is legal.
            e.HasOne(t => t.AssignedAgent)
            .WithMany(a => a.AssignedTickets)
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(t => t.Status);
            e.HasIndex(t => t.CustomerId);
        });

        modelBuilder.Entity<TicketComment>(e =>
        {
            e.Property(c => c.Body).IsRequired().HasMaxLength(4000);

            // Deleting a ticket takes its comments with it.
            e.HasOne(c => c.Ticket)
             .WithMany(t => t.Comments)
             .HasForeignKey(c => c.TicketId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.AuthorAgent)
             .WithMany()
             .HasForeignKey(c => c.AuthorAgentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tag>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(50);
            // Unique, case-insensitive: names are stored normalised to lower case
            // by TagsController, so a plain unique index is enough here.
            e.HasIndex(t => t.Name).IsUnique();
        });

        // This composite key is the rule "a ticket cannot have the same tag twice".
        modelBuilder.Entity<TicketTag>(e =>
        {
            e.HasKey(tt => new { tt.TicketId, tt.TagId });

            e.HasOne(tt => tt.Ticket)
             .WithMany(t => t.TicketTags)
             .HasForeignKey(tt => tt.TicketId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(tt => tt.Tag)
             .WithMany(t => t.TicketTags)
             .HasForeignKey(tt => tt.TagId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
    //Save ChangesAsync method implementation
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}

using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupportHub.Application.Dtos;

public record TicketCommentDto(
    int Id,
    int TicketId,
    string Body,
    int? AuthorAgentId,
    DateTime CreatedAtUtc)
{
    public static TicketCommentDto From(TicketComment comment) => new(
        comment.Id,
        comment.TicketId,
        comment.Body,
        comment.AuthorAgentId,
        DateTime.SpecifyKind(comment.CreatedAtUtc, DateTimeKind.Utc));
}

public class TicketCommentCreateDto
{
    // Required rejects null, "" and whitespace-only bodies, so "   " is a 400.
    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? AuthorAgentId { get; set; }
}

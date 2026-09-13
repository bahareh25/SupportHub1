using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Application.Services;

public class TicketCommentService : ITicketCommentService
{
   
    private IApplicationDbContext _db;
    private ILogger _logger;

    public TicketCommentService(IApplicationDbContext db, ILogger<TicketCommentService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<bool> CommentExistsAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
        return await _db.TicketComments.AnyAsync(c => c.Id == commentId && c.TicketId == ticketId, cancellationToken);
    }

    public async Task<TicketCommentDto> CreateCommentAsync(int ticketId, TicketCommentCreateDto ticketCommentCreateDto, CancellationToken cancellationToken = default)
    {
        var comment = new TicketComment
        {
            TicketId = ticketId,
            Body = ticketCommentCreateDto.Body.Trim(),
            AuthorAgentId = ticketCommentCreateDto.AuthorAgentId,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created comment {CommentId} on ticket {TicketId}", comment.Id, ticketId);
        return TicketCommentDto.From(comment);
    }

    public async Task DeleteCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
        var comment=await _db.TicketComments
            .FirstOrDefaultAsync(c=>c.Id==commentId &&  c.TicketId==ticketId, cancellationToken);
        if (comment is null)
        {
            return;
        }
        _db.TicketComments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted comment {CommentId} on ticket {TicketId}", commentId, ticketId);
    }

    public async Task<TicketCommentDto?> GetCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
      var comment= await _db.TicketComments
            .AsNoTracking()
            .FirstOrDefaultAsync(c=>c.Id==commentId && c.TicketId == ticketId, cancellationToken);
        return comment is null?null:TicketCommentDto.From(comment);
    }

    public async Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var comments =await _db.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
        return comments.Select(TicketCommentDto.From).ToList();

    }
}

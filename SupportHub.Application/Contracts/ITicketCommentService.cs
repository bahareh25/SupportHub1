using SupportHub.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Application.Contracts;

public interface ITicketCommentService
{
    Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, CancellationToken cancellationToken = default);

    // Scoped to the ticket on purpose: comment 5 of ticket 1 is not comment 5 of ticket 2.
    Task<TicketCommentDto?> GetCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default);

    Task<bool> CommentExistsAsync(int ticketId, int commentId, CancellationToken cancellationToken = default);

    Task<TicketCommentDto> CreateCommentAsync(int ticketId, TicketCommentCreateDto ticketCommentCreateDto, CancellationToken cancellationToken = default);

    Task DeleteCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default);
}

using SupportHub.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Application.Contracts;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default);

    Task<TagDto?> GetTagByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> TagExistsAsync(int id, CancellationToken cancellationToken = default);

    // Tag names are unique case-insensitively, so the name is normalised before comparing.
    Task<bool> TagNameExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<TagDto> CreateTagAsync(TagCreateDto tagCreateDto, CancellationToken cancellationToken = default);

    Task DeleteTagAsync(int id, CancellationToken cancellationToken = default);

    // Both of these are idempotent: running them twice leaves the same state.
    Task AttachTagToTicketAsync(int ticketId, int tagId, CancellationToken cancellationToken = default);

    Task DetachTagFromTicketAsync(int ticketId, int tagId, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using StajFlow.Api.DTOs;

namespace StajFlow.Api.Services;

internal static class Pagination
{
    public static async Task<PagedResult<T>> ToPageAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(items, page, pageSize, totalCount);
    }
}

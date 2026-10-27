namespace demoapi.Utils;
using demoapi.Models;
using Microsoft.EntityFrameworkCore;

public static class IQueryableExtensions
{
    public static async Task<PagedResult> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult
        {
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}


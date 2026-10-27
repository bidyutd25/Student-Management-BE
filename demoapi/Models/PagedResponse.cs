using Microsoft.AspNetCore.Mvc.RazorPages;

namespace demoapi.Models;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; init; } = Array.Empty<T>();
    public PageMetadata Pagination { get; init; } = new PageMetadata();
}
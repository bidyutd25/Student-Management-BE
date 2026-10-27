namespace demoapi.Models;

public class PagedResult
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public String sortOrder { get; set; } = "asc";
    public String sortBy { get; set; } = "Id";
}

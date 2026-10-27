
using System;

public class PageMetadata
{
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    // Optional helpers
    public int StartItem => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
    public int EndItem => Math.Min(TotalCount, PageNumber * PageSize);
}
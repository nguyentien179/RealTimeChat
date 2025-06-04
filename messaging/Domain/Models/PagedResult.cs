using System;

namespace messaging.Domain.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; } = default!;

    public int PageIndex { get; }

    public int PageSize { get; }

    public int TotalRecords { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public PagedResult(IEnumerable<T> items, int pageIndex, int pageSize, int totalRecords)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}

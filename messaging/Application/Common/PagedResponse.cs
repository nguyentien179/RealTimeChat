using System;

namespace messaging.Application.Common;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = default!;

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }
}
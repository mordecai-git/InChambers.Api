﻿using InChambers.Core.Interfaces;

namespace InChambers.Core.Models.Utilities;

public class PagedList<TDestination> : List<TDestination>, IPagedList
{
    public int PageIndex { get; set; }
    public int TotalPages { get; private set; }
    public int TotalItems { get; private set; }
    public int PageSize { get; private set; }

    public bool HasPreviousPage
    {
        get
        {
            return PageIndex > 1;
        }
    }

    public bool HasNextPage
    {
        get
        {
            return PageIndex < TotalPages;
        }
    }

    public PagedList(List<TDestination> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalItems = count;
        PageSize = pageSize;

        AddRange(items);
    }
}
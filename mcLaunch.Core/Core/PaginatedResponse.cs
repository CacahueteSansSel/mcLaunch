namespace mcLaunch.Core.Core;

public class PaginatedResponse<T>
{
    public static PaginatedResponse<T> Empty => new(0, 0, Array.Empty<T>());
    public int PageIndex { get; }
    public int TotalPageCount { get; }
    public List<T> Items { get; }
    public int Length => Items.Count;
    
    public PaginatedResponse(int pageIndex, int totalPageCount, T[] items)
    {
        PageIndex = pageIndex;
        TotalPageCount = totalPageCount;
        Items = items.ToList();
    }

    public PaginatedResponse(int pageIndex, int totalPageCount, T item)
    {
        PageIndex = pageIndex;
        TotalPageCount = totalPageCount;
        Items = [item];
    }
}
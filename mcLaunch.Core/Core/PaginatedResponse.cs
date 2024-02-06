namespace mcLaunch.Core.Core;

public class PaginatedResponse<T>
{
    public static PaginatedResponse<T> Empty => new(0, 0, Array.Empty<T>());
    public int PageIndex { get; }
    public int TotalPageCount { get; }
    public T[] Data { get; }
    
    public PaginatedResponse(int pageIndex, int totalPageCount, T[] data)
    {
        PageIndex = pageIndex;
        TotalPageCount = totalPageCount;
        Data = data;
    }

    public PaginatedResponse(int pageIndex, int totalPageCount, T item)
    {
        PageIndex = pageIndex;
        TotalPageCount = totalPageCount;
        Data = [item];
    }
}
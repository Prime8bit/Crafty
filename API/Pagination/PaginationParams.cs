namespace API.Pagination;

public class PaginationParams
{
    private const int MAX_PAGE_SIZE = 50;
    
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MAX_PAGE_SIZE ? MAX_PAGE_SIZE : value;
    }
    
    public string OrderBy { get; set; } = "";

    public bool IsOrderDescending { get; set; } = false;
}
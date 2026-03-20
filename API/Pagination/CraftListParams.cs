namespace API.Pagination;

public enum ArchiveFilterType
{
    All = 1,
    NotArchivedOnly = 2,
    ArchivedOnly = 3
}

public class CraftListParams : PaginationParams
{
    public float MaxPrice { get; set; } = float.MaxValue;
    public float MinPrice { get; set;} = 0.0f; 

    public bool InStockOnly { get; set; } = false;
    public ArchiveFilterType ArchiveFilter{ get; set; } = ArchiveFilterType.All;
}
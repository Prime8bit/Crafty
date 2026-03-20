using API.Entities;

namespace API.Pagination;

public enum OrderListType
{
    All = 0,
    SellOnly = 1,
    BuyOnly = 2
}

public class OrderListParams : PaginationParams
{
    public OrderListType TypeFilter { get; set; } = OrderListType.All;
    public bool ShowIncompleteOnly { get; set; } = false;
}
using CraftyCommon.Pagination;

namespace API.CacheKey;

public class PaginationParamsSerializer : ICacheKeyTypeSerializer<PaginationParams>
{
    public string Serialize(PaginationParams source)
    {
        return $"pp[pn:{source.PageNumber},ps:{source.PageSize},ob:{source.OrderBy},od:{(source.IsOrderDescending?"1":"0")}]";
    }
}
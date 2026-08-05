using CraftyCommon.Pagination;

namespace API.CacheKey;

public class CraftListParamsSerializer : PaginationParamsSerializer, ICacheKeyTypeSerializer<CraftListParams>
{
    public string Serialize(CraftListParams source)
    {
        return $"clp[{base.Serialize(source)},maxp:{source.MaxPrice},minp:{source.MinPrice},is:{(source.InStockOnly?"1":"0")},a:{(int)source.ArchiveFilter}]";
    }
}
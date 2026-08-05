using CraftyCommon.Pagination;

namespace API.CacheKey;

public static class CacheKeySerializer
{
    // I know this breaks the DI principle, but I can't use reflection and I don't want to move this out to Program.cs
    // so I am willing to compromise it for this.
    private static Dictionary<Type, object> _serializers = new()
    {
        { typeof(CraftListParams), new CraftListParamsSerializer() },
        { typeof(PaginationParams), new PaginationParamsSerializer() }
    };

    public static string Serialize<T>(T source)
    {
        if (!_serializers.TryGetValue(typeof(T), out var serializerObject))
            throw new InvalidOperationException($"No CacheKeySerializer registered for type {typeof(T).Name}.");

        var serializer = serializerObject as ICacheKeyTypeSerializer<T>;
        if (serializer == null)
            throw new InvalidOperationException($"No CacheKeySerializer registered for type {typeof(T).Name}.");

        return serializer.Serialize(source);
    }
}
namespace API.CacheKey;

public interface ICacheKeyTypeSerializer<T>
{
    string Serialize(T source);
}
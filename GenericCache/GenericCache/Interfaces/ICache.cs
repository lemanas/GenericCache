namespace GenericCache.Interfaces;
public interface ICache<in TParams, T>
{
    void ClearAll();
    T Get(TParams requestParams);
    void TryAdd(TParams tParams, T value);
    void Remove(TParams tParams);

    long Count();
}
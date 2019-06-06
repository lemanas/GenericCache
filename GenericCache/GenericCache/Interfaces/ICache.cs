namespace GenericCache.Interfaces
{
    public interface ICache<in TParams, T>
    {
        void ClearAll();
        T GetDataFromDictionary(TParams requestParams);
        void TryAdd(TParams tParams, T value);
        void Remove(TParams tParams);
    }
}

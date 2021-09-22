using GenericCache.Interfaces;

namespace GenericCache;
public class GenericCache<TParams, T> : GenericCacheBase<TParams, long, T>, IClearable, ICache<TParams, T>
{
    public GenericCache(int? capacity = null, List<string> ignoredParameters = null, int concurrencyLevel = 50) 
        : base(capacity, ignoredParameters, concurrencyLevel)
    {
    }

    protected override long GenerateKey(TParams requestParams)
    {
        unchecked
        {
            var enumerableObj = requestParams as System.Collections.IEnumerable;
            long key = 9973;

            if (_isKeyTypeNumericPrimitive)
            {
                return key * Convert.ToInt64(requestParams);
            }

            if (enumerableObj != null)
            {
                key = enumerableObj.GetHashCode();
            }
            else
            {
                foreach (var property in _properties)
                {
                    if (_ignoredParameters.Contains(property.Name))
                        continue;
                    var propertyValue = _executableGetter(requestParams, property);
                    if (propertyValue != null)
                        key = key * 9901 + propertyValue.GetHashCode();
                }
            }

            return key;
        }
    }
}
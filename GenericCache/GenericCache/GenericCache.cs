using System.Collections.Generic;
using GenericCache.Interfaces;

namespace GenericCache
{
    public class GenericCache<TParams, T> : IClearable, ICache<TParams, T>
    {
        private LimitedConcurrentDictionary<long, T> Cache { get; }
        private List<string> IgnoredParameters { get; }

        public GenericCache(int capacity, List<string> ignoredParameters = null)
        {
            Cache = new LimitedConcurrentDictionary<long, T>(capacity);
            IgnoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
        }

        public void ClearAll() => Cache.Clear();

        public T GetDataFromDictionary(TParams requestParams)
        {
            long key = GenerateKey(requestParams);
            var value = Cache.TryGetValue(key);

            if (value != null)
            {
                return value;
            }

            return default;
        }

        public void TryAdd(TParams tParams, T value)
        {
            long key = GenerateKey(tParams);
            if (value != null)
                Cache.TryAdd(key, value);
        }

        public void Remove(TParams tParams)
        {
            long key = GenerateKey(tParams);
            Cache.TryRemove(key);
        }

        private long GenerateKey(TParams requestParams)
        {
            unchecked
            {
                var enumerableObj = requestParams as System.Collections.IEnumerable;
                long key = 9973;

                if (enumerableObj != null)
                {
                    key = enumerableObj.GetHashCode();
                }
                else
                {
                    foreach (var property in requestParams.GetType().GetProperties())
                    {
                        if (!IgnoredParameters.Contains(property.Name))
                        {
                            object propertyValue = property.GetValue(requestParams);
                            if (propertyValue != null)
                                key = key * 9901 + propertyValue.GetHashCode();
                        }
                    }
                }

                return key;
            }
        }
    }
}

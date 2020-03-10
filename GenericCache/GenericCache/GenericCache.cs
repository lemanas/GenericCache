using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GenericCache.Interfaces;

namespace GenericCache
{
    public class GenericCache<TParams, T> : IClearable, ICache<TParams, T>
    {
        private LimitedConcurrentDictionary<long, T> Cache { get; }
        private List<string> IgnoredParameters { get; }
        private PropertyInfo[] Properties { get; }
        private Func<TParams, PropertyInfo, object> ExecutableGetter { get; }

        public GenericCache(int capacity, List<string> ignoredParameters = null)
        {
            Cache = new LimitedConcurrentDictionary<long, T>(capacity);
            IgnoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
            Properties = typeof(TParams).GetProperties();

            Expression<Func<TParams, PropertyInfo, object>> getter = (tParams, property) => property.GetValue(tParams);
            ExecutableGetter = getter.Compile();
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
                    foreach (var property in Properties)
                    {
                        if (IgnoredParameters.Contains(property.Name))
                            continue;
                        var propertyValue = ExecutableGetter(requestParams, property);
                        if (propertyValue != null)
                            key = key * 9901 + propertyValue.GetHashCode();
                    }
                }

                return key;
            }
        }
    }
}

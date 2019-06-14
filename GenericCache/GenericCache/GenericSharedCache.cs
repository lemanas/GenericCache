using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using GenericCache.Interfaces;

namespace GenericCache
{
    public class GenericSharedCache<TParams, T> : IClearable, ICache<TParams, T>
    {
        private LimitedConcurrentDictionary<Guid, T> Cache { get; }
        private List<string> IgnoredParameters { get; }
        private PropertyInfo[] Properties { get; }
        private Func<TParams, PropertyInfo, object> ExecutableGetter { get; }

        public GenericSharedCache(int capacity, List<string> ignoredParameters = null)
        {
            Cache = new LimitedConcurrentDictionary<Guid, T>(capacity);
            IgnoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
            Properties = typeof(TParams).GetProperties();

            Expression<Func<TParams, PropertyInfo, object>> getter = (tParams, property) => property.GetValue(tParams);
            ExecutableGetter = getter.Compile();
        }

        public void ClearAll() => Cache.Clear();

        public T GetDataFromDictionary(TParams requestParams)
        {
            var key = GenerateKey(requestParams);
            var value = Cache.TryGetValue(key);

            if (value != null)
            {
                return value;
            }

            return default;
        }

        public void TryAdd(TParams tParams, T value)
        {
            var key = GenerateKey(tParams);
            if (value != null)
                Cache.TryAdd(key, value);
        }

        public void Remove(TParams tParams)
        {
            var key = GenerateKey(tParams);
            Cache.TryRemove(key);
        }

        private Guid GenerateKey(TParams requestParams)
        {
            var enumerableObj = requestParams as IEnumerable;
            Guid key;

            if (enumerableObj != null)
            {
                key = new Guid(enumerableObj.ToString());
            }
            else
            {
                StringBuilder keyBuilder = new StringBuilder();
                foreach (var property in Properties)
                {
                    if (!IgnoredParameters.Contains(property.Name))
                    {
                        keyBuilder.Append($"{property.Name}=");

                        var value = ExecutableGetter(requestParams, property);
                        if (value is IEnumerable enumerable && !(enumerable is string))
                        {
                            string values = "";
                            foreach (var x in enumerable)
                            {
                                values += $", {x}";
                            }

                            keyBuilder.Append(values);
                        }
                        else
                        {
                            keyBuilder.Append(value);
                        }
                    }
                }
                key = new Guid(keyBuilder.ToString());
            }

            return key;
        }
    }
}

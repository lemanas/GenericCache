using GenericCache.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericCache
{
    public abstract class GenericCacheBase<TParams, TKey, T> : IClearable, ICache<TParams, T>
    {
        protected LimitedConcurrentDictionary<TKey, T> Cache { get; }
        protected List<string> IgnoredParameters { get; }
        protected PropertyInfo[] Properties { get; }
        protected Func<TParams, PropertyInfo, object> ExecutableGetter { get; }
        protected bool IsKeyTypeNumericPrimitive { get; }

        protected static HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public GenericCacheBase(int? capacity = null, List<string> ignoredParameters = null, int concurrencyLevel = 50)
        {
            Cache = new LimitedConcurrentDictionary<TKey, T>(capacity, concurrencyLevel);
            IgnoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
            Properties = typeof(TParams).GetProperties();

            Expression<Func<TParams, PropertyInfo, object>> getter = (tParams, property) => property.GetValue(tParams);
            ExecutableGetter = getter.Compile();

            IsKeyTypeNumericPrimitive = NumericTypes.Contains(typeof(TParams));
        }

        public void ClearAll() => Cache.Clear();

        public T Get(TParams requestParams)
        {
            TKey key = GenerateKey(requestParams);
            var value = Cache.TryGetValue(key);

            if (value != null)
            {
                return value;
            }

            return default;
        }

        public void TryAdd(TParams tParams, T value)
        {
            TKey key = GenerateKey(tParams);
            if (value != null)
                Cache.TryAdd(key, value);
        }

        public void Remove(TParams tParams)
        {
            TKey key = GenerateKey(tParams);
            Cache.TryRemove(key);
        }

        public long Count() => Cache.Count;

        protected abstract TKey GenerateKey(TParams requestParams);
    }
}

using GenericCache.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericCache
{
    public abstract class GenericCacheBase<TParams, TKey, T> : IClearable, ICache<TParams, T>
    {
        protected LimitedConcurrentDictionary<TKey, T> _cache;
        protected List<string> _ignoredParameters;
        protected PropertyInfo[] _properties;
        protected Func<TParams, PropertyInfo, object> _executableGetter;
        protected bool _isKeyTypeNumericPrimitive;

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
            _cache = new LimitedConcurrentDictionary<TKey, T>(capacity, concurrencyLevel);
            _ignoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
            _properties = typeof(TParams).GetProperties();

            Expression<Func<TParams, PropertyInfo, object>> getter = (tParams, property) => property.GetValue(tParams);
            _executableGetter = getter.Compile();

            _isKeyTypeNumericPrimitive = NumericTypes.Contains(typeof(TParams));
        }

        public void ClearAll() => _cache.Clear();

        public T Get(TParams requestParams)
        {
            TKey key = GenerateKey(requestParams);
            var value = _cache.TryGetValue(key);

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
                _cache.TryAdd(key, value);
        }

        public void Remove(TParams tParams)
        {
            TKey key = GenerateKey(tParams);
            _cache.TryRemove(key);
        }

        public long Count() => _cache.Count;

        protected abstract TKey GenerateKey(TParams requestParams);
    }
}
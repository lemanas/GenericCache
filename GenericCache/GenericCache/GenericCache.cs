using System.Linq.Expressions;
using System.Reflection;
using GenericCache.Interfaces;

namespace GenericCache;
public class GenericCache<TParams, T> : IClearable, ICache<TParams, T>
{
    private LimitedConcurrentDictionary<long, T> Cache { get; }
    private List<string> IgnoredParameters { get; }
    private PropertyInfo[] Properties { get; }
    private Func<TParams, PropertyInfo, object> ExecutableGetter { get; }
    private bool IsKeyTypeNumericPrimitive { get; }

    private static HashSet<Type> NumericTypes = new HashSet<Type>
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

    public GenericCache(int? capacity = null, List<string> ignoredParameters = null, int concurrencyLevel = 50)
    {
        Cache = new LimitedConcurrentDictionary<long, T>(capacity, concurrencyLevel);
        IgnoredParameters = ignoredParameters != null ? new List<string>(ignoredParameters) : new List<string>();
        Properties = typeof(TParams).GetProperties();

        Expression<Func<TParams, PropertyInfo, object>> getter = (tParams, property) => property.GetValue(tParams);
        ExecutableGetter = getter.Compile();

        IsKeyTypeNumericPrimitive = NumericTypes.Contains(typeof(TParams));
    }

    public void ClearAll() => Cache.Clear();

    public T Get(TParams requestParams)
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

    public long Count() => Cache.Count;

    private long GenerateKey(TParams requestParams)
    {
        unchecked
        {
            var enumerableObj = requestParams as System.Collections.IEnumerable;
            long key = 9973;

            if (IsKeyTypeNumericPrimitive)
            {
                return key * Convert.ToInt64(requestParams);
            }

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
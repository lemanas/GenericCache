using System.Collections;
using System.Security.Cryptography;
using System.Text;
using GenericCache.Interfaces;

namespace GenericCache;
public class GenericSharedCache<TParams, T> : GenericCacheBase<TParams, Guid, T>,  IClearable, ICache<TParams, T>
{
    public GenericSharedCache(int? capacity = null, List<string> ignoredParameters = null, int concurrencyLevel = 50)
        : base(capacity, ignoredParameters, concurrencyLevel)
    {
    }

    protected override Guid GenerateKey(TParams requestParams)
    {
        var enumerableObj = requestParams as IEnumerable;
        Guid key;

        if (enumerableObj != null)
        {
            key = new Guid(enumerableObj.ToString());
        }
        else
        {
            var keyBuilder = new StringBuilder();
            foreach (var property in _properties)
            {
                if (_ignoredParameters.Contains(property.Name))
                    continue;
                keyBuilder.Append($"{property.Name}=");

                var value = _executableGetter(requestParams, property);
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

            using SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.Default.GetBytes(keyBuilder.ToString()));
            return new Guid(hash);
        }

        return key;
    }
}
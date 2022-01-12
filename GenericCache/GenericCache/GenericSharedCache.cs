using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace GenericCache;

public class GenericSharedCache<TParams, T> : GenericCacheBase<TParams, Guid, T>
{
    public GenericSharedCache(int? capacity = null, List<string> ignoredParameters = null, int concurrencyLevel = 50)
        : base(capacity, ignoredParameters, concurrencyLevel)
    {
    }

    protected override Guid GenerateKey(TParams requestParams)
    {
        Guid key;

        if (requestParams is string str)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.Default.GetBytes(str));

            key = new Guid(hash);
        }
        else
        {
            string keyString;

            if (IsKeyTypeNumericPrimitive)
            {
                keyString = requestParams.ToString();
            }
            else
            {
                var keyBuilder = new StringBuilder();
                foreach (var property in Properties)
                {
                    if (IgnoredParameters.Contains(property.Name))
                        continue;
                    keyBuilder.Append($"{property.Name}=");

                    var value = ExecutableGetter(requestParams, property);
                    if (value is IEnumerable enumerable and not string)
                    {
                        var values = "";
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

                keyString = keyBuilder.ToString();
            }

            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.Default.GetBytes(keyString!));
            return new Guid(hash);
        }

        return key;
    }
}
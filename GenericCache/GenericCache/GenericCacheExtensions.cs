using System;
using System.Threading.Tasks;
using GenericCache.Interfaces;

namespace GenericCache
{
    public static class GenericCacheExtensions
    {
        public static T GetData<TParams, T>(this ICache<TParams, T> store, TParams tParams,
            Func<TParams, T> func, bool isCachingEnabled = true)
        {
            T result;
            if (isCachingEnabled)
            {
                result = store.GetDataFromDictionary(tParams);
                if (result == null)
                {
                    result = func(tParams);
                }
            }
            else
            {
                result = func(tParams);
            }

            return result;
        }

        public static async Task<T> GetDataAsync<TParams, T>(this ICache<TParams, T> store, TParams tParams,
            Func<TParams, Task<T>> func, bool isCachingEnabled = true)
        {
            T result;
            if (isCachingEnabled)
            {
                result = store.GetDataFromDictionary(tParams);
                if (result == null)
                {
                    result = await func(tParams);
                }
            }
            else
            {
                result = await func(tParams);
            }

            return result;
        }
    }
}

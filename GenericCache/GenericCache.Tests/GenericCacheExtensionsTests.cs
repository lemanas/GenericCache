using System;

namespace GenericCache.Tests
{
    public class GenericCacheExtensionsTests
    {
        private static readonly Func<int, int?> CacheAlternative = i => i * 0;
        private static readonly Func<int, Task<int?>> CacheAlternativeAsync = async i => await Task.Run(() => i * 0);

        [Fact]
        public void GetDataFromCache()
        {
            var cache = new GenericCache<int,int?>();
            cache.TryAdd(1, 1);

            var result = cache.GetData(1, CacheAlternative);

            Assert.Equal(1, result);
            Assert.NotEqual(result, CacheAlternative(1));
        }

        [Fact]
        public void GetDataFromAlternativeSource()
        {
            var cache = new GenericCache<int, int?>();

            var result = cache.GetData(0, CacheAlternative);

            Assert.Equal(1, cache.Count());
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDataWithDisabledCaching()
        {
            var cache = new GenericCache<int, int?>();

            var result = cache.GetData(0, CacheAlternative, false);

            Assert.Equal(0, cache.Count());
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetDataAsyncFromCache()
        {
            var cache = new GenericCache<int, int?>();
            cache.TryAdd(1, 1);

            var result = await cache.GetDataAsync(1, CacheAlternativeAsync);

            Assert.Equal(1, result);
            Assert.NotEqual(result, CacheAlternative(1));
        }

        [Fact]
        public async Task GetDataAsyncFromAlternativeSource()
        {
            var cache = new GenericCache<int, int?>();

            var result = await cache.GetDataAsync(1, CacheAlternativeAsync);

            Assert.Equal(1, cache.Count());
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetDataAsyncWithDisabledCaching()
        {
            var cache = new GenericCache<int, int?>();

            var result = await cache.GetDataAsync(1, CacheAlternativeAsync, false);

            Assert.Equal(0, cache.Count());
            Assert.Equal(0, result);
        }
    }
}

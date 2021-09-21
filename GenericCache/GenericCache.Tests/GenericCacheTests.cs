using Xunit;

namespace GenericCache.Tests;
public class GenericCacheTests
{
    [Fact]
    public void SimpleTryAdd()
    {
        var cache = new GenericCache<int, int>();
        cache.TryAdd(1, 1);

        Assert.Equal(1, cache.Count());
    }

    [Fact]
    public void DuplicateTryAdd()
    {
        var cache = new GenericCache<int, int>();
        var key = 1;

        cache.TryAdd(key, 1);
        cache.TryAdd(key, 1);
        cache.TryAdd(key, 2);

        Assert.Equal(1, cache.Count());
        Assert.Equal(1, cache.Get(1));
    }

    [Fact]
    public void SimpleTryAddAndGet()
    {
        var cache = new GenericCache<int, int>();
        var key = 1;
        var value = 1;

        cache.TryAdd(key, value);

        Assert.Equal(1, cache.Get(key));
    }

    [Fact]
    public void SimpleRemove()
    {
        var cache = new GenericCache<int, int?>();
        cache.TryAdd(1, 10);
        cache.TryAdd(2, 20);

        cache.Remove(1);

        Assert.Equal(1, cache.Count());
        Assert.Null(cache.Get(1));
    }

    [Fact]
    public void SimpleClear()
    {
        var cache = new GenericCache<int, int>();
        cache.TryAdd(1, 10);
        cache.TryAdd(2, 20);

        cache.ClearAll();

        Assert.Equal(0, cache.Count());
    }
}
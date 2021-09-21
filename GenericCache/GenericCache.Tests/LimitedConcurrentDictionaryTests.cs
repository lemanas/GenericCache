using Xunit;

namespace GenericCache.Tests;
public class LimitedConcurrentDictionaryTests
{
    [Fact]
    public void SimpleTryAdd()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int>();
        dictionary.TryAdd(1, 1);

        Assert.Equal(1, dictionary.Count);
    }

    [Fact]
    public void DuplicateTryAdd()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int>();
        dictionary.TryAdd(1, 1);
        dictionary.TryAdd(1, 1);
        dictionary.TryAdd(1, 2);

        Assert.Equal(1, dictionary.Count);
        Assert.Equal(1, dictionary.TryGetValue(1));
    }

    [Fact]
    public void TryAddInLimitedDictionary()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int?>(2);
        dictionary.TryAdd(1, 10);
        dictionary.TryAdd(2, 20);
        dictionary.TryAdd(3, 30);

        Assert.Equal(2, dictionary.Count);
        Assert.Null(dictionary.TryGetValue(1));
    }

    [Fact]
    public void SimpleAddOrUpdate()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int>();
        dictionary.AddOrUpdate(1, 1);

        Assert.Equal(1, dictionary.Count);
        Assert.Equal(1, dictionary.TryGetValue(1));

        dictionary.AddOrUpdate(1, 10);

        Assert.Equal(10, dictionary.TryGetValue(1));
    }

    [Fact]
    public void AddOrUpdateInLimitedDictionary()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int?>(capacity: 2);
        dictionary.AddOrUpdate(1, 10);
        dictionary.AddOrUpdate(2, 20);
        dictionary.AddOrUpdate(3, 30);
        dictionary.AddOrUpdate(4, 40);

        Assert.Equal(2, dictionary.Count);
        Assert.Null(dictionary.TryGetValue(1));

        dictionary.AddOrUpdate(2, 200);

        Assert.Equal(200, dictionary.TryGetValue(2));
        Assert.Equal(2, dictionary.Count);
    }

    [Fact]
    public void SimpleGet()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int>();
        dictionary.TryAdd(1, 1);

        Assert.Equal(1, dictionary.TryGetValue(1));
    }

    [Fact]
    public void SimpleTryRemove()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int?>();
        dictionary.TryAdd(1, 10);
        dictionary.TryAdd(2, 20);

        dictionary.TryRemove(1);

        Assert.Equal(1, dictionary.Count);
        Assert.Null(dictionary.TryGetValue(1));
    }

    [Fact]
    public void SimpleClear()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int?>();
        dictionary.TryAdd(1, 10);

        dictionary.Clear();

        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void ParallelTryAddAndGet()
    {
        var dictionary = new LimitedConcurrentDictionary<int, int?>();

        var repetitions = 100000;

        Parallel.For(1, repetitions + 1, i => dictionary.TryAdd(i, i * 10));

        Assert.Equal(repetitions, dictionary.Count);

        Parallel.For(1, repetitions + 1, i =>
        {
            Assert.Equal(i * 10, dictionary.TryGetValue(i));
        });
    }

    [Fact]
    public void ParallelTryAddAndGetLimitedDictionary()
    {
        var repetitions = 10000;
        var capacity = 1000;

        var dictionary = new LimitedConcurrentDictionary<int, int?>(capacity, repetitions);

        Parallel.For(1, repetitions + 1, i => dictionary.TryAdd(i, i * 10));

        Assert.Equal(capacity, dictionary.Count);

        Parallel.For(1, repetitions + 1, i =>
        {
            var result = dictionary.TryGetValue(i);
            Assert.True(result == i * 10 || result == null);
        });
    }
}
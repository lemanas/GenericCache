﻿using System.Collections.Generic;
using FluentAssertions;

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
    public void SimpleAddOrUpdate()
    {
        var cache = new GenericCache<int, int>();
        var key = 1;
        var value = 1;

        cache.AddOrUpdate(key, value);

        Assert.Equal(1, cache.Get(key));

        cache.AddOrUpdate(key, 2);

        Assert.Equal(2, cache.Get(key));
        Assert.Equal(1, cache.Count());
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

    [Fact]
    public void TryAddInLimitedCache()
    {
        var cache = new GenericCache<int, int?>(2);
        cache.TryAdd(1, 10);
        cache.TryAdd(2, 20);
        cache.TryAdd(3, 30);

        Assert.Equal(2, cache.Count());
        Assert.Null(cache.Get(1));
    }

    [Fact]
    public void AddOrUpdateLimitedCache()
    {
        var cache = new GenericCache<int, int?>(2);
        cache.AddOrUpdate(1, 10);
        cache.AddOrUpdate(2, 20);
        cache.AddOrUpdate(3, 30);
        cache.AddOrUpdate(4, 40);

        Assert.Equal(2, cache.Count());
        Assert.Null(cache.Get(1));

        cache.AddOrUpdate(2, 200);

        Assert.Equal(200, cache.Get(2));
        Assert.Equal(2, cache.Count());
    }

    [Fact]
    public void ParallelTryAddAndGet()
    {
        var cache = new GenericCache<int, int?>();
        var repetitions = 100000;

        Parallel.For(1, repetitions + 1, i => cache.TryAdd(i, i * 10));

        Assert.Equal(repetitions, cache.Count());

        Parallel.For(1, repetitions + 1, i =>
        {
            Assert.Equal(i * 10, cache.Get(i));
        });
    }

    [Fact]
    public void ParallelTryAddAndGetLimited()
    {
        var repetitions = 10000;
        var capacity = 1000;

        var cache = new GenericCache<int, int?>(capacity, concurrencyLevel: repetitions);

        Parallel.For(1, repetitions + 1, i => cache.TryAdd(i, i * 10));

        Assert.Equal(capacity, cache.Count());

        Parallel.For(1, repetitions + 1, i =>
        {
            var result = cache.Get(i);
            Assert.True(result == i * 10 || result == null);
        });
    }

    [Fact]
    public void TryAddWithStringParamType()
    {
        var cache = new GenericCache<string, int>();

        cache.TryAdd("1", 1);
        Assert.Equal(1, cache.Count());
    }

    [Fact]
    public void TryAddAndGetWithStringParamType()
    {
        var cache = new GenericCache<string, int>();

        cache.TryAdd("1", 1);

        Assert.Equal(1, cache.Get("1"));
    }

    [Fact]
    public void DuplicateTryAddWithStringParamType()
    {
        var cache = new GenericCache<string, int>();

        var key = "1";

        cache.TryAdd(key, 1);
        cache.TryAdd(key, 1);
        cache.TryAdd(key, 2);

        Assert.Equal(1, cache.Count());
        Assert.Equal(1, cache.Get("1"));
    }

    [Fact]
    public void TryAddWithComplexParamType()
    {
        var cache = new GenericCache<ComplexType, int>();
        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(key, 1);
        Assert.Equal(1, cache.Count());
    }

    [Fact]
    public void TryAddAndGetWithComplexParamType()
    {
        var cache = new GenericCache<ComplexType, int>();
        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(key, 1);

        Assert.Equal(1, cache.Get(key));
    }

    [Fact]
    public void DuplicateTryAddAndGetWithComplexParamType()
    {
        var cache = new GenericCache<ComplexType, int>();

        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(key, 1);
        cache.TryAdd(key, 1);
        cache.TryAdd(key, 2);

        Assert.Equal(1, cache.Count());
        Assert.Equal(1, cache.Get(key));
    }

    [Fact]
    public void ComplexTypeKeyGeneration()
    {
        var cache = new GenericCache<ComplexType, int>();

        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        var key2 = new ComplexType
        {
            Id = 2,
            Name = "SomeName2",
            Location = "Somewhere1"
        };

        cache.TryAdd(key, 1);
        cache.TryAdd(key2, 1);

        Assert.Equal(2, cache.Count());
        Assert.Equal(cache.Get(key), cache.Get(key2));
    }

    [Fact]
    public void Test_ComplexTypeKeyGeneration_WithIgnoredParameters()
    {
        var cache = new GenericCache<ComplexType, int>(ignoredParameters: new List<string> { "Id" });

        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        var key2 = new ComplexType
        {
            Id = 2,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(key, 1);
        cache.TryAdd(key2, 1);

        Assert.Equal(1, cache.Count());
    }

    [Fact]
    public void EqualityByValueComplexTypeKeyGeneration()
    {
        var cache = new GenericCache<ComplexType, int>();

        var key = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        var key2 = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(key, 1);
        cache.TryAdd(key2, 1);

        Assert.Equal(1, cache.Count());
    }

    [Fact]
    public void ComplexTypeAsValue()
    {
        var cache = new GenericCache<int, ComplexType>();

        var value = new ComplexType
        {
            Id = 1,
            Name = "SomeName",
            Location = "Somewhere"
        };

        cache.TryAdd(1, value);
        cache.TryAdd(2, value);

        Assert.Equal(2, cache.Count());

        var cachedValue = cache.Get(1);

        cachedValue.Should().BeEquivalentTo(value);
    }

    private class ComplexType
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public string? Location { get; init; }
    }
}
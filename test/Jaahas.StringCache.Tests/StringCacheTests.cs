using System.Collections.Concurrent;

namespace Jaahas.StringCacheTests;

public class StringCacheTests {

    [Fact]
    public void Constructor_Default_CreatesCustomCache() {
        // Arrange & Act
        var cache = new StringCache();

        // Assert
        Assert.False(cache.NativeInternEnabled);
        Assert.Equal(0, cache.Count);
    }


    [Fact]
    public void SharedInstance_IsCustomCache() {
        // Arrange & Act
        var shared = StringCache.Shared;

        // Assert
        Assert.False(shared.NativeInternEnabled);
        Assert.True(shared.Count >= 0);
    }


    [Fact]
    public void NativeInstance_IsNativeInterning() {
        // Arrange & Act
        var native = StringCache.Native;

        // Assert
        Assert.True(native.NativeInternEnabled);
        Assert.Equal(-1, native.Count);
    }


    [Fact]
    public void Intern_WithValidString_CustomCache_ReturnsString() {
        // Arrange
        var cache = new StringCache();
        const string testString = "test string";

        // Act
        var result = cache.Intern(testString);

        // Assert
        Assert.Equal(testString, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_SameStringTwice_CustomCache_ReturnsSameInstance() {
        // Arrange
        var cache = new StringCache();
        const string testString = "test string";

        // Act
        var result1 = cache.Intern(testString);
        var result2 = cache.Intern(testString);

        // Assert
        Assert.Same(result1, result2);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_DifferentStrings_CustomCache_CachesEachString() {
        // Arrange
        var cache = new StringCache();
        const string string1 = "test string 1";
        const string string2 = "test string 2";

        // Act
        var result1 = cache.Intern(string1);
        var result2 = cache.Intern(string2);

        // Assert
        Assert.Equal(string1, result1);
        Assert.Equal(string2, result2);
        Assert.Equal(2, cache.Count);
    }


    [Fact]
    public void Clear_CustomCache_RemovesAllStrings() {
        // Arrange
        var cache = new StringCache();
        cache.Intern("string1");
        cache.Intern("string2");
        Assert.Equal(2, cache.Count);

        // Act
        cache.Clear();

        // Assert
        Assert.Equal(0, cache.Count);
    }


    [Fact]
    public void Clear_NativeCache_DoesNothing() {
        // Arrange & Act
        StringCache.Native.Clear();

        // Assert - Should not throw, count should still be -1
        Assert.Equal(-1, StringCache.Native.Count);
    }


    [Fact]
    public void Intern_WithNull_ReturnsNull() {
        // Arrange
        var cache = new StringCache();

        // Act
        var result = cache.Intern(null!);

        // Assert
        Assert.Null(result);
        Assert.Equal(0, cache.Count);
    }


    [Fact]
    public void Intern_WithEmptyString_ReturnsEmptyString() {
        // Arrange
        var cache = new StringCache();

        // Act
        var result = cache.Intern(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_WithWhitespace_ReturnsWhitespace() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "   ";

        // Act
        var result = cache.Intern(whitespace);

        // Assert
        Assert.Equal(whitespace, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_WithTabsAndNewlines_ReturnsAsIs() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "\t\n\r ";

        // Act
        var result = cache.Intern(whitespace);

        // Assert
        Assert.Equal(whitespace, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_NativeCache_WithValidString_ReturnsInternedString() {
        // Arrange
        var cache = StringCache.Native;
        const string testString = "native test string";

        // Act
        var result = cache.Intern(testString);

        // Assert
        Assert.Equal(testString, result);
        Assert.Same(result, string.Intern(testString));
    }


    [Fact]
    public void Intern_NativeCache_WithNull_ReturnsNull() {
        // Arrange
        var cache = StringCache.Native;

        // Act
        var result = cache.Intern(null!);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Intern_NativeCache_WithEmptyString_ReturnsEmptyString() {
        // Arrange
        var cache = StringCache.Native;

        // Act
        var result = cache.Intern(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public async Task Intern_ConcurrentAccess_CustomCache_ThreadSafe() {
        // Arrange
        var cache = new StringCache();
        const int threadCount = 10;
        const int operationsPerThread = 100;
        var tasks = new Task[threadCount];
        var results = new ConcurrentBag<string>();

        // Act
        for (var i = 0; i < threadCount; i++) {
            var threadId = i;
            tasks[threadId] = Task.Run(() => {
                for (var j = 0; j < operationsPerThread; j++) {
                    var testString = $"thread-{threadId}-string-{j % 5}"; // Reuse some strings
                    var result = cache.Intern(testString);
                    results.Add(result);
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(threadCount * operationsPerThread, results.Count);

        // Verify that identical strings from different threads are the same instance
        var groupedResults = results.GroupBy(s => s).ToList();
        foreach (var group in groupedResults) {
            var instances = group.ToList();
            for (var i = 1; i < instances.Count; i++) {
                Assert.Same(instances[0], instances[i]);
            }
        }
    }


    [Fact]
    public void Intern_StringWithSpecialCharacters_WorksCorrectly() {
        // Arrange
        var cache = new StringCache();
        const string specialString = "Hello 世界! 🌟 Test@#$%^&*()";

        // Act
        var result1 = cache.Intern(specialString);
        var result2 = cache.Intern(specialString);

        // Assert
        Assert.Equal(specialString, result1);
        Assert.Same(result1, result2);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_VeryLongString_WorksCorrectly() {
        // Arrange
        var cache = new StringCache();
        var longString = new string('a', 10000);

        // Act
        var result1 = cache.Intern(longString);
        var result2 = cache.Intern(longString);

        // Assert
        Assert.Equal(longString, result1);
        Assert.Same(result1, result2);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Intern_CaseSensitive_TreatsAsDifferentStrings() {
        // Arrange
        var cache = new StringCache();
        const string lowerCase = "test string";
        const string upperCase = "TEST STRING";
        const string mixedCase = "Test String";

        // Act
        var result1 = cache.Intern(lowerCase);
        var result2 = cache.Intern(upperCase);
        var result3 = cache.Intern(mixedCase);

        // Assert
        Assert.Equal(lowerCase, result1);
        Assert.Equal(upperCase, result2);
        Assert.Equal(mixedCase, result3);
        Assert.NotSame(result1, result2);
        Assert.NotSame(result1, result3);
        Assert.NotSame(result2, result3);
        Assert.Equal(3, cache.Count);
    }


    [Fact]
    public void StaticInstances_AreSingleton() {
        // Arrange & Act
        var shared1 = StringCache.Shared;
        var shared2 = StringCache.Shared;
        var native1 = StringCache.Native;
        var native2 = StringCache.Native;

        // Assert
        Assert.Same(shared1, shared2);
        Assert.Same(native1, native2);
        Assert.NotSame(shared1, native1);
    }


    [Fact]
    public void Get_WithNonCachedString_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();
        const string testString = "not cached string";

        // Act
        var result = cache.Get(testString);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithCachedString_CustomCache_ReturnsString() {
        // Arrange
        var cache = new StringCache();
        const string testString = "cached string";
        cache.Intern(testString);

        // Act
        var result = cache.Get(testString);

        // Assert
        Assert.Equal(testString, result);
        Assert.Same(testString, result);
    }


    [Fact]
    public void Get_AfterClearingCache_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();
        const string testString = "cached string";
        cache.Intern(testString);
        cache.Clear();

        // Act
        var result = cache.Get(testString);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithMultipleStrings_CustomCache_ReturnsCorrectStrings() {
        // Arrange
        var cache = new StringCache();
        const string string1 = "first string";
        const string string2 = "second string";
        const string string3 = "third string";
        
        cache.Intern(string1);
        cache.Intern(string3);

        // Act
        var result1 = cache.Get(string1);
        var result2 = cache.Get(string2);
        var result3 = cache.Get(string3);

        // Assert
        Assert.Equal(string1, result1);
        Assert.Null(result2);
        Assert.Equal(string3, result3);
    }


    [Fact]
    public void Get_WithNull_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();

        // Act
        var result = cache.Get(null!);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithEmptyString_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();

        // Act
        var result = cache.Get(string.Empty);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithWhitespace_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "   ";

        // Act
        var result = cache.Get(whitespace);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithTabsAndNewlines_CustomCache_ReturnsNull() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "\t\n\r ";

        // Act
        var result = cache.Get(whitespace);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithNonInternedString_NativeCache_ReturnsNull() {
        // Arrange
        var cache = StringCache.Native;
        // Create a string that is guaranteed not to be interned by constructing it dynamically
        var baseString = "non_interned_";
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var testString = new string((baseString + suffix).ToCharArray());

        // Act
        var result = cache.Get(testString);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithInternedString_NativeCache_ReturnsInternedString() {
        // Arrange
        var cache = StringCache.Native;
        const string testString = "interned string for get test";
        string.Intern(testString);

        // Act
        var result = cache.Get(testString);

        // Assert
        Assert.Equal(testString, result);
        Assert.Same(testString, result);
    }


    [Fact]
    public void Get_WithLiteralString_NativeCache_ReturnsString() {
        // Arrange
        var cache = StringCache.Native;
        const string literalString = "literal string";

        // Act
        var result = cache.Get(literalString);

        // Assert
        Assert.Equal(literalString, result);
        Assert.Same(literalString, result);
    }


    [Fact]
    public void Get_WithNull_NativeCache_ReturnsNull() {
        // Arrange
        var cache = StringCache.Native;

        // Act
        var result = cache.Get(null!);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void Get_WithEmptyString_NativeCache_ReturnsEmptyString() {
        // Arrange
        var cache = StringCache.Native;

        // Act
        var result = cache.Get(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public void Get_WithWhitespace_NativeCache_ReturnsWhitespace() {
        // Arrange
        var cache = StringCache.Native;
        const string whitespace = "   ";

        // Act
        var result = cache.Get(whitespace);

        // Assert
        Assert.Equal(whitespace, result);
    }


    [Fact]
    public async Task Get_ConcurrentAccess_CustomCache_ThreadSafe() {
        // Arrange
        var cache = new StringCache();
        const int threadCount = 5;
        const int operationsPerThread = 50;
        var testStrings = Enumerable.Range(0, 10).Select(i => $"test-string-{i}").ToArray();
        
        foreach (var str in testStrings) {
            cache.Intern(str);
        }

        var tasks = new Task[threadCount];
        var results = new ConcurrentBag<(string input, string? output)>();

        // Act
        for (var i = 0; i < threadCount; i++) {
            tasks[i] = Task.Run(() => {
                for (var j = 0; j < operationsPerThread; j++) {
                    var testString = testStrings[j % testStrings.Length];
                    var result = cache.Get(testString);
                    results.Add((testString, result));
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(threadCount * operationsPerThread, results.Count);
        foreach (var (input, output) in results) {
            Assert.Equal(input, output);
            Assert.Same(input, output);
        }
    }


    [Fact]
    public void Get_BehaviorConsistentAcrossFrameworks_CustomCache() {
        // Arrange
        var cache = new StringCache();
        const string cachedString = "framework test string";
        const string nonCachedString = "non cached framework test string";
        
        cache.Intern(cachedString);

        // Act & Assert - Testing that Get method behavior is consistent
        // regardless of whether NET8_0_OR_GREATER compilation path is used
        var cachedResult = cache.Get(cachedString);
        var nonCachedResult = cache.Get(nonCachedString);

        // Assert
        Assert.NotNull(cachedResult);
        Assert.Equal(cachedString, cachedResult);
        Assert.Same(cachedString, cachedResult);
        
        Assert.Null(nonCachedResult);
    }


    [Fact]
    public void Get_WithLargeDataSet_CustomCache_PerformsCorrectly() {
        // Arrange
        var cache = new StringCache();
        var testData = Enumerable.Range(0, 1000)
            .Select(i => $"performance-test-string-{i}")
            .ToArray();
        
        // Cache every other string
        for (var i = 0; i < testData.Length; i += 2) {
            cache.Intern(testData[i]);
        }

        // Act & Assert
        for (var i = 0; i < testData.Length; i++) {
            var result = cache.Get(testData[i]);
            
            if (i % 2 == 0) {
                // Should be cached
                Assert.NotNull(result);
                Assert.Same(testData[i], result);
            } else {
                // Should not be cached
                Assert.Null(result);
            }
        }
        
        // Verify cache count
        Assert.Equal(500, cache.Count);
    }


    [Fact]
    public void Get_And_Intern_Integration_CustomCache_WorkTogether() {
        // Arrange
        var cache = new StringCache();
        const string testString = "integration test string";

        // Act & Assert - Initially not cached
        var initialResult = cache.Get(testString);
        Assert.Null(initialResult);
        Assert.Equal(0, cache.Count);

        // Intern the string
        var internedResult = cache.Intern(testString);
        Assert.Equal(testString, internedResult);
        Assert.Equal(1, cache.Count);

        // Now Get should return the cached string
        var cachedResult = cache.Get(testString);
        Assert.NotNull(cachedResult);
        Assert.Same(internedResult, cachedResult);
        Assert.Same(testString, cachedResult);

        // Clear and verify Get returns null again
        cache.Clear();
        var clearedResult = cache.Get(testString);
        Assert.Null(clearedResult);
        Assert.Equal(0, cache.Count);
    }


    [Fact]
    public void Get_WithCachedEmptyString_CustomCache_ReturnsEmptyString() {
        // Arrange
        var cache = new StringCache();
        cache.Intern(string.Empty);

        // Act
        var result = cache.Get(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
        Assert.Same(string.Empty, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Get_WithCachedWhitespace_CustomCache_ReturnsWhitespace() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "   ";
        cache.Intern(whitespace);

        // Act
        var result = cache.Get(whitespace);

        // Assert
        Assert.Equal(whitespace, result);
        Assert.Same(whitespace, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void Get_WithCachedTabsAndNewlines_CustomCache_ReturnsAsIs() {
        // Arrange
        var cache = new StringCache();
        const string whitespace = "\t\n\r ";
        cache.Intern(whitespace);

        // Act
        var result = cache.Get(whitespace);

        // Assert
        Assert.Equal(whitespace, result);
        Assert.Same(whitespace, result);
        Assert.Equal(1, cache.Count);
    }


    [Fact]
    public void CalculateSize_EmptyCache_CustomCache_ReturnsZero() {
        // Arrange
        var cache = new StringCache();

        // Act
        var size = cache.CalculateSize();

        // Assert
        Assert.Equal(0L, size);
    }


    [Fact]
    public void CalculateSize_NativeCache_ReturnsMinusOne() {
        // Arrange
        var cache = StringCache.Native;

        // Act
        var size = cache.CalculateSize();

        // Assert
        Assert.Equal(-1L, size);
    }


    [Fact]
    public void CalculateSize_SingleString_CustomCache_ReturnsCorrectSize() {
        // Arrange
        var cache = new StringCache();
        const string testString = "hello";
        cache.Intern(testString);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = testString.Length * sizeof(char);
        Assert.Equal(expectedSize, size);
    }


    [Fact]
    public void CalculateSize_MultipleStrings_CustomCache_ReturnsCorrectTotalSize() {
        // Arrange
        var cache = new StringCache();
        const string string1 = "hello";
        const string string2 = "world";
        const string string3 = "test";
        
        cache.Intern(string1);
        cache.Intern(string2);
        cache.Intern(string3);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = (string1.Length + string2.Length + string3.Length) * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(3, cache.Count);
    }


    [Fact]
    public void CalculateSize_WithEmptyString_CustomCache_IncludesInCalculation() {
        // Arrange
        var cache = new StringCache();
        const string normalString = "hello";
        var emptyString = string.Empty;
        
        cache.Intern(normalString);
        cache.Intern(emptyString);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = (normalString.Length + emptyString.Length) * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(2, cache.Count);
    }


    [Fact]
    public void CalculateSize_WithWhitespaceStrings_CustomCache_ReturnsCorrectSize() {
        // Arrange
        var cache = new StringCache();
        const string spaces = "   ";
        const string tabs = "\t\t";
        const string newlines = "\n\r";
        
        cache.Intern(spaces);
        cache.Intern(tabs);
        cache.Intern(newlines);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = (spaces.Length + tabs.Length + newlines.Length) * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(3, cache.Count);
    }


    [Fact]
    public void CalculateSize_WithUnicodeStrings_CustomCache_ReturnsCorrectSize() {
        // Arrange
        var cache = new StringCache();
        const string unicode1 = "Hello 世界";
        const string unicode2 = "🌟✨";
        const string unicode3 = "café";
        
        cache.Intern(unicode1);
        cache.Intern(unicode2);
        cache.Intern(unicode3);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = (unicode1.Length + unicode2.Length + unicode3.Length) * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(3, cache.Count);
    }


    [Fact]
    public void CalculateSize_AfterClear_CustomCache_ReturnsZero() {
        // Arrange
        var cache = new StringCache();
        cache.Intern("hello");
        cache.Intern("world");
        Assert.True(cache.CalculateSize() > 0);

        // Act
        cache.Clear();
        var size = cache.CalculateSize();

        // Assert
        Assert.Equal(0L, size);
        Assert.Equal(0, cache.Count);
    }


    [Fact]
    public void CalculateSize_DuplicateStrings_CustomCache_CountsOnce() {
        // Arrange
        var cache = new StringCache();
        const string testString = "duplicate";
        
        cache.Intern(testString);
        cache.Intern(testString); // Same string interned twice

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = testString.Length * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(1, cache.Count); // Should only be counted once
    }


    [Fact]
    public void CalculateSize_LargeString_CustomCache_ReturnsCorrectSize() {
        // Arrange
        var cache = new StringCache();
        var largeString = new string('x', 10000);
        cache.Intern(largeString);

        // Act
        var size = cache.CalculateSize();

        // Assert
        var expectedSize = largeString.Length * sizeof(char);
        Assert.Equal(expectedSize, size);
        Assert.Equal(20000L, size); // 10000 chars * 2 bytes each
    }


    [Fact]
    public async Task CalculateSize_ConcurrentAccess_CustomCache_ThreadSafe() {
        // Arrange
        var cache = new StringCache();
        const int threadCount = 5;
        const int stringsPerThread = 20;
        var tasks = new Task[threadCount];

        // Act - Add strings concurrently
        for (int i = 0; i < threadCount; i++) {
            int threadId = i;
            tasks[i] = Task.Run(() => {
                for (int j = 0; j < stringsPerThread; j++) {
                    cache.Intern($"thread-{threadId}-string-{j}");
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        // Calculate size after all strings are added
        var size = cache.CalculateSize();

        // Assert
        Assert.True(size > 0);
        Assert.True(cache.Count > 0);
        Assert.True(cache.Count <= threadCount * stringsPerThread); // Some strings might be duplicates
    }

}

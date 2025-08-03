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
        Assert.Equal(0, cache.Count);
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
        Assert.Equal(0, cache.Count);
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
        Assert.Equal(0, cache.Count);
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
        for (int i = 0; i < threadCount; i++) {
            int threadId = i;
            tasks[threadId] = Task.Run(() => {
                for (int j = 0; j < operationsPerThread; j++) {
                    var testString = $"thread-{threadId}-string-{j % 5}"; // Reuse some strings
                    var result = cache.Intern(testString);
                    results.Add(result);
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(threadCount * operationsPerThread, results.Count);

        // Verify that identical strings from different threads are the same instance
        var groupedResults = results.GroupBy(s => s).ToList();
        foreach (var group in groupedResults) {
            var instances = group.ToList();
            for (int i = 1; i < instances.Count; i++) {
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

}

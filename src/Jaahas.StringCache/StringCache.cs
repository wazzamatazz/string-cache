using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Jaahas;

/// <summary>
/// A cache for string instances.
/// </summary>
/// <remarks>
///
/// <para>
///   This class provides a way to cache strings to avoid unnecessary allocations in a more
///   performant way than using built-in .NET string interning.
/// </para>
///
/// <para>
///   A shared instance of this cache can be accessed via the <see cref="Shared"/> property. The
///   <see cref="Native"/> property can be used to intern strings using the built-in <see cref="string.Intern(string)"/>
///   method.
/// </para>
///
/// <para>
///   The number of strings in the cache can be accessed via the <see cref="Count"/> property.
/// </para>
/// 
/// </remarks>
public class StringCache {

    /// <summary>
    /// The shared instance of the string cache.
    /// </summary>
    public static StringCache Shared { get; } = new StringCache(false);
    
    /// <summary>
    /// The instance of the string cache that uses built-in .NET string interning.
    /// </summary>
    public static StringCache Native { get; } = new StringCache(true);
    
    /// <summary>
    /// The internal cache for string instances.
    /// </summary>
    private readonly ConcurrentDictionary<string, string>? _cache;
    
    /// <summary>
    /// Specifies whether the cache uses built-in .NET string interning.
    /// </summary>
    public bool NativeInternEnabled => _cache == null;
    
    /// <summary>
    /// The number of strings in the cache.
    /// </summary>
    /// <remarks>
    ///   This property returns -1 when native interning is enabled.
    /// </remarks>
    public int Count => _cache?.Count ?? -1;


    /// <summary>
    /// Creates a new <see cref="StringCache"/> instance.
    /// </summary>
    /// <param name="useNativeIntern">
    ///   Specifies whether to use built-in .NET string interning.
    /// </param>
    private StringCache(bool useNativeIntern) {
        if (!useNativeIntern) {
            _cache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        }
    }
    
    
    /// <summary>
    /// Creates a new <see cref="StringCache"/> instance.
    /// </summary>
    public StringCache() : this(false) {}

    
    /// <summary>
    /// Interns the specified string.
    /// </summary>
    /// <param name="str">
    ///   The string to intern.
    /// </param>
    /// <returns>
    ///   The interned string. If <paramref name="str"/> is <see langword="null"/>, it is returned
    ///   as-is.
    /// </returns>
    [return: NotNullIfNotNull(nameof(str))]
    public string? Intern(string? str) {
        if (str == null) {
            return null;
        }

        return NativeInternEnabled
            ? string.Intern(str) 
            : _cache!.GetOrAdd(str, str);
    }
    
    
    /// <summary>
    /// Gets a reference to the interned string if it exists in the cache.
    /// </summary>
    /// <param name="str">
    ///   The string to retrieve from the cache.
    /// </param>
    /// <returns>
    ///   The interned string if it exists in the cache; otherwise, <see langword="null"/>.
    /// </returns>
    public string? Get(string? str) {
        if (str == null) {
            return null;
        }
        
#if NET8_0_OR_GREATER
        return NativeInternEnabled
            ? string.IsInterned(str)
            : _cache!.GetValueOrDefault(str);
#else
        return NativeInternEnabled
            ? string.IsInterned(str)
            : _cache!.TryGetValue(str, out var cachedValue)
                ? cachedValue
                : null;
#endif
    }
    
    
    /// <summary>
    /// Removes all strings from the cache.
    /// </summary>
    public void Clear() {
        if (NativeInternEnabled) {
            return;
        }

        _cache!.Clear();
    }


    /// <summary>
    /// Calculates the total size of the interned strings in bytes.
    /// </summary>
    /// <returns>
    ///   The total size of the cached strings in bytes. Returns -1 when native interning is enabled.
    /// </returns>
    /// <remarks>
    ///   This method calculates the size based on UTF-16 encoding (2 bytes per character).
    /// </remarks>
    public long CalculateSize() {
        if (NativeInternEnabled) {
            return -1;
        }

        long totalSize = 0;
        foreach (var key in _cache!.Keys) {
            totalSize += key.Length * sizeof(char);
        }

        return totalSize;
    }

}

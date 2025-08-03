# About

Jaahas.StringCache provides a string cache that can provide a more performant alternative to .NET's built-in `string.Intern()` method. This library is inspired by Sergey Teplyakov's blog post on [string interning performance](https://sergeyteplyakov.github.io/Blog/benchmarking/2023/12/10/Intern_or_Not_Intern.html).

The library offers both custom string caching using `ConcurrentDictionary` and a wrapper around native string interning, allowing you to choose the approach that best fits your performance requirements.

# How to Use

## Using the Shared Instance

```csharp
using Jaahas;

// Cache strings using the shared instance
var cached1 = StringCache.Shared.Intern("Hello World");
var cached2 = StringCache.Shared.Intern("Hello World");

// Both variables reference the same string instance
Console.WriteLine(ReferenceEquals(cached1, cached2)); // True
```

## Using Native Interning

```csharp
using Jaahas;

// Use native .NET string interning
var interned = StringCache.Native.Intern("Hello World");
```

# Advanced Usage

## Creating Custom Instances

For scenarios where you need isolated caching or want to manage cache lifecycle:

```csharp
using Jaahas;

var cache = new StringCache();
var cached = cache.Intern("Custom cache string");

// Check cache size
Console.WriteLine($"Cache contains {cache.Count} strings");

// Clear the cache when needed
cache.Clear();
```

Note: The `Count` property returns -1 for the `Native` instance, and calling `Clear()` has no effect.

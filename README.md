# Jaahas.StringCache

A .NET string cache that can be used as an alternative to calling `string.Intern`. The concept for this library is taken from Sergey Teplyakov's blog post [here](https://sergeyteplyakov.github.io/Blog/benchmarking/2023/12/10/Intern_or_Not_Intern.html).

The library offers both custom string caching using `ConcurrentDictionary<TKey, TValue>` and a wrapper around native string interning, allowing you to choose the approach that best fits your requirements.


# Usage

```csharp
// Cache strings using the shared instance
var cached1 = Jaahas.StringCache.Shared.Intern("Hello, World!");
var cached2 = Jaahas.StringCache.Shared.Intern("Hello, World!");

// Both variables reference the same string instance
Console.WriteLine(ReferenceEquals(cached1, cached2)); // True

// Check cache size
Console.WriteLine($"Cache contains {Jaahas.StringCache.Shared.Count} strings");

// Clear the cache when needed
Jaahas.StringCache.Shared.Clear();
```


# Benchmarks

Benchmarks can be found in the [Jaahas.StringCache.Benchmarks](./test/Jaahas.StringCache.Benchmarks) project. The benchmarks compare the performance of the `Jaahas.StringCache` class against the built-in `string.Intern` method.

Example results from running the benchmarks on an Apple M4 Pro with .NET 9.0.7:

```
BenchmarkDotNet v0.15.2, macOS Sequoia 15.6 (24G84) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.303
  [Host]     : .NET 9.0.7 (9.0.725.31616), Arm64 RyuJIT AdvSIMD [AttachedDebugger]
  DefaultJob : .NET 9.0.7 (9.0.725.31616), Arm64 RyuJIT AdvSIMD


| Method          | Count   | Mean          | Error        | StdDev       | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------- |-------- |--------------:|-------------:|-------------:|------:|--------:|-------:|----------:|------------:|
| NativeInterning | 10000   |   1,137.35 us |    22.721 us |    21.253 us |  1.00 |    0.03 |      - |   6.34 KB |        1.00 |
| CacheInterning  | 10000   |      35.51 us |     0.701 us |     1.005 us |  0.03 |    0.00 | 0.5493 |   4.57 KB |        0.72 |
|                 |         |               |              |              |       |         |        |           |             |
| NativeInterning | 100000  |   9,536.63 us |   104.627 us |    97.868 us |  1.00 |    0.01 |      - |   6.32 KB |        1.00 |
| CacheInterning  | 100000  |     195.14 us |     0.955 us |     0.893 us |  0.02 |    0.00 | 0.4883 |   4.62 KB |        0.73 |
|                 |         |               |              |              |       |         |        |           |             |
| NativeInterning | 1000000 | 146,965.17 us | 1,643.612 us | 1,537.436 us |  1.00 |    0.01 |      - |   5.09 KB |        1.00 |
| CacheInterning  | 1000000 |   4,066.65 us |    33.637 us |    31.464 us |  0.03 |    0.00 |      - |   4.93 KB |        0.97 |

```


# Building the Solution

The repository uses [Cake](https://cakebuild.net/) for cross-platform build automation. The build script allows for metadata such as a build counter to be specified when called by a continuous integration system such as TeamCity.

A build can be run from the command line using the [build.ps1](/build.ps1) PowerShell script or the [build.sh](/build.sh) Bash script. For documentation about the available build script parameters, see [build.cake](/build.cake).


# Software Bill of Materials

To generate a Software Bill of Materials (SBOM) for the repository in [CycloneDX](https://cyclonedx.org/) XML format, run [build.ps1](./build.ps1) or [build.sh](./build.sh) with the `--target BillOfMaterials` parameter.

The resulting SBOM is written to the `artifacts/bom` folder.

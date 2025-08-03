# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building the Solution
```bash
# Build the solution (runs tests by default)
./build.sh

# Build with specific configuration
./build.sh --configuration=Release

# Clean build
./build.sh --clean

# Skip tests during build
./build.sh --no-tests

# Build specific targets
./build.sh --target=Pack          # Build and create NuGet packages
./build.sh --target=Build         # Build only
./build.sh --target=Test          # Run tests only
./build.sh --target=BillOfMaterials  # Generate SBOM
```

### Testing
```bash
# Run all tests
./build.sh --target=Test

# Run tests for a specific project
dotnet test test/Jaahas.StringCache.Tests/

# Run benchmarks
dotnet run --project test/Jaahas.StringCache.Benchmarks/
```

### Package Management
The repository uses NuGet Central Package Management. Package versions are centrally managed in `Directory.Packages.props`. When adding new packages, only include `<PackageReference Include="PackageName" />` in project files without version numbers.

## Architecture and Code Structure

### Project Structure
```
src/Jaahas.StringCache/           # Main library project
├── StringCache.cs                # Core string caching implementation
└── Jaahas.StringCache.csproj     # Multi-target (net9.0, netstandard2.0)

test/Jaahas.StringCache.Tests/    # Unit tests (xUnit framework)
test/Jaahas.StringCache.Benchmarks/  # Performance benchmarks (BenchmarkDotNet)
```

### Core Implementation
The `StringCache` class provides:
- **Shared instance**: `StringCache.Shared` - thread-safe custom string cache using `ConcurrentDictionary<string, string>`
- **Native instance**: `StringCache.Native` - wrapper around built-in `string.Intern()`
- **Custom instances**: Create new instances with `new StringCache()`

Key methods:
- `Intern(string str)` - Caches/retrieves strings, returns input unchanged for null/whitespace
- `Clear()` - Clears custom cache (no-op for native interning)
- `Count` property - Number of cached strings (-1 for native interning)

### Build System
Uses [Cake Build](https://cakebuild.net/) with `Jaahas.Cake.Extensions` for cross-platform automation. The build script (`build.cake`) supports versioning, signing, containerization, and SBOM generation.

## Development Guidelines

### Testing Framework
- Uses **xUnit** for unit tests, not MSTest
- Test projects target .NET 9.0
- Include coverage collection with coverlet.collector
- Tests should be isolated and independent

### Code Style
- Multi-targeting: Main library supports .NET 9.0 and .NET Standard 2.0
- Uses latest C# language features with nullable reference types enabled
- Follows existing XML documentation patterns with British English spelling
- Thread-safe implementation using `ConcurrentDictionary` for custom caching
- Performance-focused design as alternative to built-in string interning
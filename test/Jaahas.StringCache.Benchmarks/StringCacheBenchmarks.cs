using BenchmarkDotNet.Attributes;

namespace Jaahas.StringCacheBenchmarks;

[MemoryDiagnoser]
public class StringCacheBenchmarks {

    [Params(1_000, 10_000, 100_000)]
    public int Count { get; set; }


    [Benchmark(Baseline = true)]
    public void NoInterning() {
        for (var i = 0; i < Count; i++) {
            _ = new Observation("Front Door", $"Value-{i % 100}");
        }
    }
    
    
    [Benchmark]
    public void NativeInterning() {
        for (var i = 0; i < Count; i++) {
            _ = new Observation(string.Intern("Front Door"), string.Intern($"Value-{i % 100}"));
        }
    }
    
    
    [Benchmark]
    public void CacheInterning() {
        for (var i = 0; i < Count; i++) {
            _ = new Observation(StringCache.Shared.Intern("Front Door"), StringCache.Shared.Intern($"Value-{i % 100}"));
        }
    }
    

    private readonly record struct Observation(string Location, string Value);

}

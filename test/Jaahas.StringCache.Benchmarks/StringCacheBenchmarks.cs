using BenchmarkDotNet.Attributes;

namespace Jaahas.StringCacheBenchmarks;

[MemoryDiagnoser]
public class StringCacheBenchmarks {
    
    [Params(10_000, 100_000, 1_000_000)]
    public int Count { get; set; }

    private List<string> _stringsToIntern = null!;


    [GlobalSetup]
    public void Setup() {
        _stringsToIntern = new List<string>(Count);
        var runId = Guid.CreateVersion7(TimeProvider.System.GetUtcNow());
        for (var i = 0; i < Count; i++) {
            _stringsToIntern.Add(string.Concat(i.ToString(), "_", runId));
        }
    }
    
    
    [GlobalCleanup]
    public void Cleanup() {
        _stringsToIntern.Clear();
        StringCache.Shared.Clear();
    }
    
    
    [Benchmark(Baseline = true)]
    public void NativeInterning() {
        Parallel.ForEach(_stringsToIntern, x => string.Intern(x));
    }
    
    
    [Benchmark]
    public void CacheInterning() {
        Parallel.ForEach(_stringsToIntern, x => StringCache.Shared.Intern(x));
    }

}

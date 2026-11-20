# DsaLab: data structures and algorithms in C# / .NET 10

[![CI](https://github.com/miclone/DsaLab/actions/workflows/ci.yml/badge.svg)](https://github.com/miclone/DsaLab/actions/workflows/ci.yml)

```
DsaLab.slnx                  solution (new .slnx format)
Directory.Build.props        net10.0, nullable, implicit usings for all projects
src/Dsa/                     the library: everything from DsaReference.cs, now public
tests/Dsa.Tests/             xUnit tests (one or more per algorithm)
benchmarks/Dsa.Benchmarks/   BenchmarkDotNet: sorts, collections, graph algorithms
samples/                     runnable single-file demos (dotnet run <file>.cs)
```

## Samples

Single-file programs that run without a project (.NET 10 file-based apps):

```powershell
cd samples
dotnet run DsaReference.cs              # every data structure and algorithm, with a printed demo of each
dotnet run ConcurrentCollectionsDemo.cs # ConcurrentDictionary/Queue/Stack/Bag, BlockingCollection, Channel<T>
```

The concurrency demo checks its own results and also shows the common mistakes:
`List<T>` silently losing items under parallel writes (71% lost on a 12-thread CPU),
and `dict[key]++` losing updates even on a `ConcurrentDictionary` (82% lost).

## Run the tests

```powershell
dotnet test
```

Most tests are **property tests**: rather than checking one hand-picked answer, they
compare an algorithm against a slow-but-obviously-correct version on many random inputs:

| Algorithm                  | Checked against                              |
|----------------------------|----------------------------------------------|
| Every sort                 | `Array.Sort` on 7 input shapes (empty, sorted, reversed, duplicates, random…) |
| MinHeap                    | built-in `PriorityQueue`                     |
| Dijkstra / Bellman-Ford    | each other and Floyd-Warshall, on random graphs |
| Kruskal                    | Prim, on random graphs                       |
| Segment / Fenwick tree     | summing the array slice directly             |
| KMP / Rabin-Karp           | naive substring search                       |
| Knapsack                   | trying every subset                          |
| LIS, Kadane                | the O(n²) versions                           |
| Sieve, ModPow              | trial division, `BigInteger.ModPow`          |

## Run the benchmarks

Always use Release, otherwise BenchmarkDotNet refuses to run:

```powershell
cd benchmarks/Dsa.Benchmarks
dotnet run -c Release                          # shows a menu
dotnet run -c Release -- --filter *Sorting*    # just the sorts
dotnet run -c Release -- --filter *Collection* # custom vs built-in collections
dotnet run -c Release -- --filter *Graph*      # shortest paths and MSTs
```

The sorting run takes a few minutes because Bubble, Selection and Insertion sort on
10,000 items are slow. That slowness is the point. Results land in
`BenchmarkDotNet.Artifacts/results/` as Markdown, HTML and CSV.

What to look for:
- **Ratio column:** how many times slower than the baseline (`Array.Sort`,
  or the built-in collection). Expect O(n²) sorts to get roughly 100× worse when N grows 10×,
  and O(n log n) sorts about 13× worse.
- **Allocated column:** Merge sort and Counting sort allocate extra memory; Quick and Heap don't.

## Package versions

The test and benchmark projects pin known-good versions (xUnit 2.9.3, BenchmarkDotNet 0.15.2).
To move to the latest, run `dotnet add package <name>` inside that project folder.
